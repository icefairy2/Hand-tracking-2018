using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace raw_streams.cs
{
    public class UpdateStatusEventArgs : EventArgs
    {
        public String text { get; set; }

        public UpdateStatusEventArgs(String text)
        {
            this.text = text;
        }
    }

    public class RenderFrameEventArgs : EventArgs
    {
        public int index { get; set; }
        public PXCMImage image { get; set; }

        public RenderFrameEventArgs(int index, PXCMImage image)
        {
            this.index = index;
            this.image = image;
        }
    }
}
