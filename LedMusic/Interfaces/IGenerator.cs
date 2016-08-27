using LedMusic.Models;

namespace LedMusic.Interfaces
{
    public interface IGenerator
    {

        Color[] getSample(int frameNumber);

    }
}
