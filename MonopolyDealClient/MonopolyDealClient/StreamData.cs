using System;
using System.Collections.Generic;
using System.Text;

namespace SocketDLL
{
    public class StreamData
    {
        public String timeStamp;
        public StreamData(String _timeStamp)
        {
            timeStamp = _timeStamp;
        }

        virtual public void printData()
        {
        }

        virtual public StreamData decode(byte[] byteStream)
        {
            return new StreamData("00:00:00");
        }

        virtual public byte[] encode()
        {
            return new byte[5];
        }

        virtual public StreamData data()
        {
            return new StreamData("00:00:00");
        }

        //virtual public double Duration
        //{
        //    get
        //    {
        //        return 0.0;
        //    }
        //}

        //virtual public int Blink
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //    set
        //    {
        //    }
        //}

        //virtual public float XPosition
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public float YPosition
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

       

        //virtual public double Img_Mag
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double ECG
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double H_Sound
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double PPG
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public GSRData GSR
        //{
        //    get
        //    {
        //        return new GSRData(0,0,0,0,0,"0:0:0");
        //    }
        //}

        //virtual public double SKT
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double EMG_C
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double EMG_Z
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double EMG_T
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Tonic_Mean
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Tonic_Slope
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Response_Rate
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Mean_Amp
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Max_Amp
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        //virtual public double Amp
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

    } // End StreamData class
} // End Namespace
