using REPRODUCTOR_MUSICAL.Models;

namespace REPRODUCTOR_MUSICAL.Graphics
{
    public interface IVisualizer
    {
        string Name { get; }

        void Update(AudioFrame audioFrame);

        void Render(System.Drawing.Graphics graphics, System.Drawing.Rectangle bounds);
    }
}
