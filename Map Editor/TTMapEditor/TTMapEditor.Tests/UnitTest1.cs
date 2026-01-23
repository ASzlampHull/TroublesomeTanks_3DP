using Microsoft.Xna.Framework;
using TTMapEditor.Managers;
using TTMapEditor.Objects;
using System.Text.Json;
using Xunit;


namespace TTMapEditor.Tests
{
        internal class TestSceneObject : SceneObject
        {
            public TestSceneObject(int x, int y, int w = 10, int h = 10) : base(null, new Rectangle(x, y, w, h))
            {

            }
        }

        public class DraggableTemplateTests
        {
            [Fact]
            public void BeginDrag_sets_offset_an_original_rect_and_flag()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);

                tpl.BeginDrag(new Vector2(110, 60));

                Assert.True(tpl.mIsDragging);
                Assert.Equal(new Vector2(10, 10), tpl.mDragOffset);
                Assert.Equal(new Rectangle(100, 50, 10, 10), tpl.mOriginalRect);
            }

            [Fact]
            public void Update_moves_template_while_dragging()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 100));
                Assert.Equal(new Rectangle(140, 90, 10, 10), obj.mRectangle);
            }

            [Fact]
            public void EndDrag_returns_final_and_resets_when_requested()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 120));
                var final = tpl.EndDrag(true);

                Assert.Equal(new Rectangle(140, 110, 10, 10), final);
                Assert.Equal(new Rectangle(100, 50, 10, 10), obj.mRectangle);
                Assert.False(tpl.mIsDragging);
            }

            [Fact]
            public void Reset_restores_original_and_clears_state()
            {
                var obj = new TestSceneObject(100, 50);
                var tpl = new DraggableTemplate<TestSceneObject>(obj);
                tpl.BeginDrag(new Vector2(110, 60));
                tpl.Update(new Vector2(150, 120));
                tpl.Reset();

                Assert.Equal(new Rectangle(100, 50, 10, 10), obj.mRectangle);
                Assert.False(tpl.mIsDragging);
                Assert.Equal(Vector2.Zero, tpl.mDragOffset);
            }
        }

        public class SceneObjectTests
        {
            [Fact]
            public void UpdatePosition_updates_rectangle_and_outline()
            {
                var obj = new TestSceneObject(10, 20, 30, 40);
                obj.UpdatePosition(50, 60);

                Assert.Equal(new Rectangle(50, 60, 30, 40), obj.mRectangle);
                // OutlinePad = 2 => outline = rect expanded by 2 on each side
                var outlineField = typeof(SceneObject).GetField("mOutlineRectangle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(outlineField);
                var outline = (Rectangle)outlineField.GetValue(obj);
                Assert.Equal(new Rectangle(48, 58, 34, 44), outline);
            }

            [Fact]
            public void SetRectangle_updates_outline_correctly()
            {
                var obj = new TestSceneObject(0, 0, 10, 10);
                obj.SetRectangle(new Rectangle(5, 5, 2, 2));
                Assert.Equal(new Rectangle(5, 5, 2, 2), obj.mRectangle);
                // expected outline: X-2,Y-2, W+4,H+4 => (3,3,6,6)
                var outlineField = typeof(SceneObject).GetField("mOutlineRectangle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(outlineField);
                var outline = (Rectangle)outlineField.GetValue(obj);
                Assert.Equal(new Rectangle(3, 3, 6, 6), outline);
            }

            [Fact]
            public void IsPointWithin_returns_expected()
            {
                var obj = new TestSceneObject(10, 10, 20, 20);
                Assert.True(obj.IsPointWithin(new Vector2(15, 15)));
                Assert.False(obj.IsPointWithin(new Vector2(100, 100)));
            }

            [Fact]
            public void Selection_helpers_toggle_and_report_state()
            {
                var obj = new TestSceneObject(0, 0, 1, 1);
                Assert.False(obj.GetIsSelected());
                obj.ToggleSelected();
                Assert.True(obj.GetIsSelected());
                obj.SetSelected(false);
                Assert.False(obj.GetIsSelected());
            }
        }

        public class RectWallTests
        {
            [Fact]
            public void ScaleHeight_increases_height_and_ceil_and_minimum_1()
            {
                var wall = new RectWall(null, new Rectangle(0, 0, 10, 10));
                wall.ScaleHeight(1.5f); // 10 * 1.5 = 15
                Assert.Equal(15, wall.mRectangle.Height);

                var small = new RectWall(null, new Rectangle(0, 0, 5, 1));
                small.ScaleHeight(0.0001f); // should never go below 1
                Assert.True(small.mRectangle.Height >= 1);
            }

            [Fact]
            public void ScaleWidth_increases_width_and_ceil_and_minimum_1()
            {
                var wall = new RectWall(null, new Rectangle(0, 0, 10, 10));
                wall.ScaleWidth(1.6f); // 10 * 1.6 = 16 -> ceil 16
                Assert.Equal(16, wall.mRectangle.Width);

                var small = new RectWall(null, new Rectangle(0, 0, 1, 5));
                small.ScaleWidth(0.0001f);
                Assert.True(small.mRectangle.Width >= 1);
            }
        }

        public class TankTests
        {
            [Fact]
            public void Constructor_sets_initial_rotation_and_rectangle()
            {
                var rect = new Rectangle(5, 6, 12, 14);
                var tank = new Tank(null, rect);

                Assert.Equal(0f, tank.Rotation);
                Assert.Equal(rect, tank.mRectangle);
            }

            [Fact]
            public void Rotate_adds_delta_to_rotation()
            {
                var tank = new Tank(null, new Rectangle(0, 0, 10, 10));
                tank.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI / 2f, tank.Rotation);
                tank.Rotate((float)System.Math.PI / 2f);
                Assert.Equal((float)System.Math.PI, tank.Rotation);
            }
        }

        public class PickupTests
        {
            [Fact]
            public void Constructor_sets_rectangle()
            {
                var rect = new Rectangle(1, 2, 3, 4);
                var pick = new Pickup(null, rect);
                Assert.Equal(rect, pick.mRectangle);
            }
        }

        public class MapManagerTests : IDisposable
        {
            private readonly string _origCwd;
            private readonly string _tempRoot;

            public MapManagerTests()
            {
                _origCwd = Environment.CurrentDirectory;
                _tempRoot = Path.Combine(Path.GetTempPath(), "TTMapEditorTests_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(_tempRoot);
                Environment.CurrentDirectory = _tempRoot;
            }

            public void Dispose()
            {
                try
                {
                    Environment.CurrentDirectory = _origCwd;
                    if (Directory.Exists(_tempRoot))
                        Directory.Delete(_tempRoot, recursive: true);
                }
                catch
                {
                    // best-effort cleanup
                }
            }

            [Fact]
            public void CreateNewMap_creates_maps_folder_and_map_json_with_empty_contents()
            {
                // Act
                string createdName = MapManager.createNewMap("MyMap");

                // Assert
                Assert.Equal("MyMap", createdName);

                string mapsDir = Path.Combine(_tempRoot, "Maps");
                string mapDir = Path.Combine(mapsDir, "MyMap");
                Assert.True(Directory.Exists(mapsDir));
                Assert.True(Directory.Exists(mapDir));

                string mapFile = Path.Combine(mapDir, "map.json");
                Assert.True(File.Exists(mapFile));

                var json = File.ReadAllText(mapFile);
                var mapData = JsonSerializer.Deserialize<MapData>(json);
                Assert.NotNull(mapData);
                Assert.NotNull(mapData.Walls);
                Assert.NotNull(mapData.Tanks);
                Assert.NotNull(mapData.Pickups);
                Assert.Empty(mapData.Walls);
                Assert.Empty(mapData.Tanks);
                Assert.Empty(mapData.Pickups);
            }

            [Fact]
            public void CreateNewMap_appends_suffix_when_folder_exists_and_sanitizes_name()
            {
                // Prepare an existing folder with base name
                string mapsDir = Path.Combine(_tempRoot, "Maps");
                Directory.CreateDirectory(mapsDir);
                string baseName = "MyMap";
                string existing = Path.Combine(mapsDir, baseName);
                Directory.CreateDirectory(existing);

                // Call with same name -> should return "MyMap (1)" and create that folder
                string created = MapManager.createNewMap(baseName);
                Assert.Equal("MyMap (1)", created);

                string createdDir = Path.Combine(mapsDir, created);
                Assert.True(Directory.Exists(createdDir));
                Assert.True(File.Exists(Path.Combine(createdDir, "map.json")));
            }

            [Fact]
            public void CreateNewMap_throws_for_empty_or_whitespace_name()
            {
                Assert.Throws<ArgumentException>(() => MapManager.createNewMap(""));
                Assert.Throws<ArgumentException>(() => MapManager.createNewMap("   "));
            }
        }
    }