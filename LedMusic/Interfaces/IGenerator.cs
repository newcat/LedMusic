using LedMusic.Models;

namespace LedMusic.Interfaces
{
    public interface IGenerator
    {

        string GeneratorName { get; }
        Color[] getSample(int frameNumber);

    }
}
