using System.IO;
using System.Threading.Tasks;

namespace Programmer.Programmer
{
    internal interface IProgrammer
    {
        Task ProgramHexFile(Stream stream);
    }
}