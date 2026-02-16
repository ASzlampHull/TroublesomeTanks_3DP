using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TTMapEditor.Maps;
using TTMapEditor.Objects;

namespace TTMapEditor.Managers
{
    public class SelectionManager
    {
        SceneObject mSelectedObject;

        Rectangle mSelectedObjectPreviousRect;

        Vector2 mSelectedDragOffset;

        MapPreview mMapPreview;

        MapBoundaryValidator mMapBoundaryValidator;

        public SelectionManager(MapPreview pPreview, MapBoundaryValidator pValidator)
        {
            mMapPreview = pPreview;
            mMapBoundaryValidator = pValidator;
        }

        public void DeselectAll()
        {
            foreach (var w in mMapPreview.GetWalls()) w.SetSelected(false);
            foreach (var t in mMapPreview.GetTanks()) t.SetSelected(false);
            foreach (var p in mMapPreview.GetPickups()) p.SetSelected(false);
            mSelectedObject = null;
        }

        public void HandleInteraction(Vector2 pMousePosition)
        {
            bool handledClick = false;

            // pick-ups (top-most)
            HandleSelectionFor(mMapPreview.GetPickups(), ref handledClick, pMousePosition);

            // tanks
            if (!handledClick) HandleSelectionFor(mMapPreview.GetTanks(), ref handledClick, pMousePosition);

            // walls
            if (!handledClick) HandleSelectionFor(mMapPreview.GetWalls(), ref handledClick, pMousePosition);

            // On mouse release finalize move: if object outside play area revert
            if (mSelectedObject != null && mSelectedObject.GetIsSelected() && InputManager.isLeftMouseReleased())
            {
                if (mSelectedObject is RectWall rw && !mMapBoundaryValidator.IsWallWithinPlayArea(rw))
                {
                    mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                }
                else if (mSelectedObject is Tank || mSelectedObject is Pickup)
                {
                    Rectangle r = mSelectedObject.mRectangle;
                    if (!mMapBoundaryValidator.IsRectWithinPlayArea(r))
                    {
                        mSelectedObject.SetRectangle(mSelectedObjectPreviousRect);
                    }
                }

                mSelectedObject.SetSelected(false);
                mSelectedObject = null;
            }
        }

        public SceneObject GetSelectedObject()
        {
            return mSelectedObject;
        }

        public void SetSelectedObject(SceneObject pSceneObject)
        {
            mSelectedObject = pSceneObject;
        }

        /// <summary>
        /// Generic selection/dragging logic for lists of SceneObject-derived types.
        /// </summary>
        public void HandleSelectionFor<T>(List<T> list, ref bool handledClick, Vector2 mousePos) where T : SceneObject
        {
            foreach (T obj in list)
            {
                if (handledClick) break;

                if (obj.IsPointWithin(mousePos) && InputManager.isLeftMouseClicked())
                {
                    if (!obj.GetIsSelected())
                    {
                        DeselectAll();
                        obj.SetSelected(true);
                        mSelectedObject = obj;
                        mSelectedObjectPreviousRect = obj.mRectangle;
                        mSelectedDragOffset = new Vector2(mousePos.X - obj.mRectangle.X, mousePos.Y - obj.mRectangle.Y);
                    }
                    else
                    {
                        obj.SetSelected(false);
                        mSelectedObject = null;
                    }
                    handledClick = true;
                }

                if (obj.GetIsSelected() && !InputManager.isLeftMouseReleased())
                {
                    int newX = (int)(mousePos.X - mSelectedDragOffset.X);
                    int newY = (int)(mousePos.Y - mSelectedDragOffset.Y);

                    // Store previous position before updating
                    Rectangle previousRect = obj.mRectangle;

                    obj.UpdatePosition(newX, newY);

                    // Check if the new position is valid
                    bool isValid = true;
                    if (obj is RectWall wall)
                    {
                        isValid = mMapBoundaryValidator.IsWallWithinPlayArea(wall);
                    }
                    else
                    {
                        isValid = mMapBoundaryValidator.IsRectWithinPlayArea(obj.mRectangle);
                    }

                    // If invalid, revert to previous position
                    if (!isValid)
                    {
                        obj.SetRectangle(previousRect);
                    }
                    else
                    {
                        // Update the stored previous rect for successful moves
                        mSelectedObjectPreviousRect = obj.mRectangle;
                    }
                }
            }
        }

    }
}
