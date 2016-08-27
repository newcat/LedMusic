using LedMusic.Models;
using System.Collections.ObjectModel;

namespace LedMusic.Interfaces
{
    interface IGenerator
    {

        Color[] getSample(int frameNumber);

    }
}
