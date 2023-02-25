using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Resources.DxProxy.Dx
{
    public class DxElement
    {
        //private readonly Dx dx;

        public Guid Id { get; set; } = Guid.NewGuid();

        private Vector2 position;
        public Vector2 Position 
        {
            get => this.position;
            set 
            { 
                position = value; 
                // TODO: tigger an Update method
            } 
        }

        private Vector2 size;
        public Vector2 Size 
        {
            get => this.position;
            set 
            { 
                position = value; 
                // TODO: tigger an Update method
            } 
        }
    }
}
