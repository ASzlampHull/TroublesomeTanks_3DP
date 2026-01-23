using Microsoft.Xna.Framework;

namespace TTMapEditor.Objects
{
    /// <summary>
    /// Lightweight container for template drag state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct DraggableTemplate<T> where T : SceneObject
    {
        public T mTemplate { get; }
        public Rectangle mOriginalRect { get; private set; }

        public bool mIsDragging { get; private set; }

        public Vector2 mDragOffset { get; private set; }

        public DraggableTemplate(T pTemplate)
        {
            mTemplate = pTemplate;
            mOriginalRect = pTemplate.mRectangle;
            mIsDragging = false;
            mDragOffset = Vector2.Zero;
        }

        
        /// <summary>
        /// Start Dragging: captire original rect and offset from mouse.
        /// Caller should check click/conditions before calling.
        /// </summary>
        public void BeginDrag(Vector2 pMousePosition)
        {
            mOriginalRect = mTemplate.mRectangle;
            mDragOffset = new Vector2(pMousePosition.X - mOriginalRect.X, pMousePosition.Y - mOriginalRect.Y);
            mIsDragging = true;
        }

        
        /// <summary>
        /// Update template position while dragging.
        /// </summary>
        public void Update(Vector2 pMousePosiition)
        {
            if (!mIsDragging) return;
            int newX = (int)(pMousePosiition.X - mDragOffset.X);
            int newY = (int)(pMousePosiition.Y - mDragOffset.Y);
            mTemplate.UpdatePosition(newX, newY);
        }

        /// <summary>
        /// End the drag. Returns final rectangle. If pResetToOriginal is true, resets template to original rectangle.
        /// </summary>
        public Rectangle EndDrag(bool pResetToOriginal = true)
        {
            if(!mIsDragging) return mTemplate.mRectangle;
            Rectangle final = mTemplate.mRectangle;
            if(pResetToOriginal)
            {
                mTemplate.SetRectangle(mOriginalRect);
            }
            mIsDragging = false;
            return final;
        }

        /// <summary>
        /// Reset template to stored original rectangle and clear drag state.
        /// </summary>
        public void Reset()
        {
            mTemplate.SetRectangle(mOriginalRect);
            mIsDragging = false;
            mDragOffset = Vector2.Zero;
        }
    }
}
