using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Managers;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor
{
    public class EditorKeyboardController
    {

        private readonly MapPreview mMapPreview;
        private readonly SelectionManager mSelectionManager;
        private readonly MapBoundaryValidator mMapBoundaryValidator;

        private Rectangle mLastValidRect;

        public EditorKeyboardController(MapPreview pMapPreview, SelectionManager pSelectionManager, MapBoundaryValidator pMapBoundaryValidator)
        {
            mMapPreview = pMapPreview;
            mSelectionManager = pSelectionManager;
            mMapBoundaryValidator = pMapBoundaryValidator;
        }

        public void Update()
        {
            SceneObject selected = mSelectionManager.GetSelectedObject();
            if (selected == null || !selected.GetIsSelected())
            {
                return;
            }

            // Delete
            if (InputManager.isKeyPressed(Keys.Delete))
            {
                mMapPreview.RemoveObject(selected);
                mSelectionManager.SetSelectedObject(null);
                return;
            }

            // Cache last valid rect for walls
            if (selected is RectWall)
            {
                mLastValidRect = selected.mRectangle;
            }

            if(selected is Pickup)
            {
                if (InputManager.isKeyPressed(Keys.D1))
                {
                    ((Pickup)selected).TogglePickupType(PickupType.HEALTH);
                }
                if (InputManager.isKeyPressed(Keys.D2))
                {
                    ((Pickup)selected).TogglePickupType(PickupType.EMP);
                }
                if (InputManager.isKeyPressed(Keys.D3))
                {
                    ((Pickup)selected).TogglePickupType(PickupType.MINE);
                }
                if (InputManager.isKeyPressed(Keys.D4))
                {
                    ((Pickup)selected).TogglePickupType(PickupType.BOUNCY_BULLET);
                }
            }

            HandleTankRotation(selected as Tank);
            HandleWallTransform(selected as RectWall);
        }

        private static void HandleTankRotation(Tank pTank)
        {
            if (pTank == null)
            {
                return;
            }

            float rotationStep = MathHelper.ToRadians(15.0f);

            if (InputManager.isKeyPressed(Keys.Left))
            {
                pTank.Rotate(-rotationStep);
            }

            if (InputManager.isKeyPressed(Keys.Right))
            {
                pTank.Rotate(rotationStep);
            }
        }

        private void HandleWallTransform(RectWall pWall)
        {
            if (pWall == null)
            {
                return;
            }

            if (InputManager.isKeyPressed(Keys.LeftControl) || InputManager.isKeyPressed(Keys.RightControl))
            {
                pWall.SwitchRotationScaling();
            }

            if (pWall.GetIsRotating())
            {
                HandleWallRotation(pWall);
            }
            else
            {
                HandleWallScaling(pWall);
            }
        }

        private void HandleWallRotation(RectWall pWall)
        {
            float rotationStep = MathHelper.ToRadians(15.0f);
            float previousRotation = pWall.mRotation;

            if (InputManager.isKeyPressed(Keys.Left))
            {
                pWall.Rotate(rotationStep);
                if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
                {
                    pWall.mRotation = previousRotation;
                }
                else
                {
                    mLastValidRect = pWall.mRectangle;
                }
            }

            if (InputManager.isKeyPressed(Keys.Right))
            {
                pWall.Rotate(-rotationStep);
                if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
                {
                    pWall.mRotation = previousRotation;
                }
                else
                {
                    mLastValidRect = pWall.mRectangle;
                }
            }
        }

        private void HandleWallScaling(RectWall pWall)
        {
            if (InputManager.isKeyPressed(Keys.Left))
            {
                pWall.ScaleWidth(0.75f);
            }
            if (InputManager.isKeyPressed(Keys.Right))
            {
                pWall.ScaleWidth(1.25f);
            }
            if (InputManager.isKeyPressed(Keys.Up))
            {
                pWall.ScaleHeight(1.25f);
            }
            if (InputManager.isKeyPressed(Keys.Down))
            {
                pWall.ScaleHeight(0.75f);
            }

            if (!mMapBoundaryValidator.IsWallWithinPlayArea(pWall))
            {
                pWall.SetWallRectangle(mLastValidRect);
            }
            else
            {
                mLastValidRect = pWall.mRectangle;
            }
        }





    }
}
