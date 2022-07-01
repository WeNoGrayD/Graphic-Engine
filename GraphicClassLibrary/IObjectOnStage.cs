using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicClassLibrary
{
    /* Интерфейс, который обязывает объекты иметь возможность
       изменять локальные и прочие координаты своих точек. */

    public interface IObjectOnStage
    {
        string Name { get; set; }

        Scene Stage { get; set; }

        void UpdatePoints(char mode, 
            Dictionary<string, double[,]> UpdateMatrices);
    }
}
