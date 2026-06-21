using System.Collections.Generic;

namespace proyectoPaint.GraphicsCore
{
    // Controller in the MVC separation: it owns document mutations and history.
    public sealed class DocumentController
    {
        private readonly CanvasDocument document;
        private readonly Stack<DrawableShape> redoShapes = new Stack<DrawableShape>();

        public DocumentController(CanvasDocument document)
        {
            this.document = document;
        }

        public void Add(DrawableShape shape)
        {
            document.Shapes.Add(shape);
            redoShapes.Clear();
        }

        public bool Undo()
        {
            if (document.Shapes.Count == 0) return false;
            int last = document.Shapes.Count - 1;
            redoShapes.Push(document.Shapes[last]);
            document.Shapes.RemoveAt(last);
            return true;
        }

        public bool Redo()
        {
            if (redoShapes.Count == 0) return false;
            document.Shapes.Add(redoShapes.Pop());
            return true;
        }

        public void Clear()
        {
            document.Clear();
            redoShapes.Clear();
        }

        public bool Remove(DrawableShape shape)
        {
            if (shape == null || !document.Shapes.Remove(shape)) return false;
            redoShapes.Clear();
            return true;
        }

        public bool BringToFront(DrawableShape shape)
        {
            if (shape == null || !document.Shapes.Remove(shape)) return false;
            document.Shapes.Add(shape);
            return true;
        }
    }
}
