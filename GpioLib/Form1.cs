using Susi4.APIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GpioLib
{
    public class IO
    {

        #region 私有变量
        const int MAX_BANK_NUM = 4;
        List<DeviceInfo> DevList = new List<DeviceInfo>();
        DeviceInfo Dev = null;
        List<DevPinInfo> DevPinList = new List<DevPinInfo>();
        DevPinInfo DevPin = null;
        #endregion
        #region 私有类
        class DeviceInfo
        {
            public UInt32 ID;
            public UInt32 SupportInput;
            public UInt32 SupportOutput;

            public DeviceInfo(UInt32 DeviceID)
            {
                ID = DeviceID;
                SupportInput = 0;
                SupportOutput = 0;
            }
        }
        class DevPinInfo
        {
            public UInt32 ID;

            private string _Name = "";
            public string Name
            {
                get { return _Name; }
            }

            override public string ToString()
            {
                return String.Format("{0} ({1})", ID, Name);
            }

            public DevPinInfo(UInt32 DeviceID)
            {
                ID = DeviceID;

                UInt32 Length = 32;
                StringBuilder sb = new StringBuilder((int)Length);
                if (SusiBoard.SusiBoardGetStringA(SusiBoard.SUSI_ID_MAPPING_GET_NAME_GPIO(ID), sb, ref Length) == SusiStatus.SUSI_STATUS_SUCCESS)
                {
                    _Name = sb.ToString();
                }
            }
        }
        #endregion
        #region 不常用的私有方法
        /// <summary>
        /// 初始化GPIO
        /// </summary>
        private void InitializeGPIO()
        {
            UInt32 Status;

            for (int i = 0; i < MAX_BANK_NUM; i++)
            {
                DeviceInfo info = new DeviceInfo(SusiGPIO.SUSI_ID_GPIO_BANK((UInt32)i));

                Status = SusiGPIO.SusiGPIOGetCaps(info.ID, SusiGPIO.SUSI_ID_GPIO_INPUT_SUPPORT, out info.SupportInput);
                if (Status != SusiStatus.SUSI_STATUS_SUCCESS)
                    continue;

                Status = SusiGPIO.SusiGPIOGetCaps(info.ID, SusiGPIO.SUSI_ID_GPIO_OUTPUT_SUPPORT, out info.SupportOutput);
                if (Status != SusiStatus.SUSI_STATUS_SUCCESS)
                    continue;

                DevList.Add(info);

            }

            if (DevList.Count > 0)
            {

            }
        }
        /// <summary>
        /// 初始化引脚
        /// </summary>
        private void InitializePins()
        {
            StringBuilder sb = new StringBuilder(32);
            UInt32 mask;

            for (int i = 0; i < DevList.Count; i++)
            {
                // 32 pins per bank
                for (int j = 0; j < 32; j++)
                {
                    mask = (UInt32)(1 << j);
                    if ((DevList[i].SupportInput & mask) > 0 || (DevList[i].SupportOutput & mask) > 0)
                    {
                        DevPinInfo pinInfo = new DevPinInfo((UInt32)((i << 5) + j));
                        DevPinList.Add(pinInfo);
                    }
                }
            }
            if (DevList.Count > 0)
            {
            }
        }
        /// <summary>
        /// 获取pin脚Id
        /// </summary>
        /// <returns></returns>
        private UInt32 GetID()
        {
            return DevPin.ID;
        }
        /// <summary>
        /// 获取Mask
        /// </summary>
        /// <returns></returns>
        private UInt32 GetMask()
        {
            return 1;
        }
        #endregion
        public IO()
        {
            try
            {
                UInt32 Status = SusiLib.SusiLibInitialize();
                if (Status != SusiStatus.SUSI_STATUS_SUCCESS && Status != SusiStatus.SUSI_STATUS_INITIALIZED)
                    return;
            }
            catch
            {
                return;
            }
            InitializeGPIO();
            InitializePins();
        }

        /// <summary>
        /// 设置引脚为输入还是输出
        /// </summary>
        /// <param name="pinNum">引脚Id</param>
        /// <param name="state">设置引脚是输入或者输出，只能是0/1</param>
        public void SetDirection(uint pinNum, OnOffState state)
        {
            DevPin = DevPinList[(int)pinNum];
            UInt32 Status;
            //只能是0或1
            UInt32 Value = (UInt32)state;
            Status = SusiGPIO.SusiGPIOSetDirection(GetID(), GetMask(), Value);
            if (Status != SusiStatus.SUSI_STATUS_SUCCESS)
                MessageBox.Show(String.Format("SusiGPIOSetDirection() failed. (0x{0:X8})", Status));
        }
        /// <summary>
        /// 设置信号电压
        /// </summary>
        /// <param name="PinNum">引脚Id</param>
        /// <param name="level">设置输出的电压，只能是0/1</param>
        public void SetLevel(uint pinNum, OnOffState level)
        {
            DevPin = DevPinList[(int)pinNum];
            UInt32 Status;
            UInt32 Value = (UInt32)level;
            Status = SusiGPIO.SusiGPIOSetLevel(GetID(), GetMask(), Value);
            if (Status != SusiStatus.SUSI_STATUS_SUCCESS)
                MessageBox.Show(String.Format("SusiGPIOSetLevel() failed. (0x{0:X8})", Status));
        }
        /// <summary>
        /// 获取电压信号
        /// </summary>
        /// <param name="PinNum">引脚id</param>
        /// <returns>返回对应引脚的电压状态</returns>
        public uint GetLevel(uint pinNum)
        {
            DevPin = DevPinList[(int)pinNum];

            UInt32 Status;
            UInt32 Value;

            Status = SusiGPIO.SusiGPIOGetLevel(GetID(), GetMask(), out Value);
            if (Status == SusiStatus.SUSI_STATUS_SUCCESS)
            {
                return Value;
            }
            else
            {
                MessageBox.Show(String.Format("SusiGPIOGetLevel() failed. (0x{0:X8})", Status));
                return 999;
            }
        }

        /// <summary>
        /// On或者Off状态
        /// </summary>
        public enum OnOffState
        {
            On = 1,
            Off = 0
        }

    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    }
}
