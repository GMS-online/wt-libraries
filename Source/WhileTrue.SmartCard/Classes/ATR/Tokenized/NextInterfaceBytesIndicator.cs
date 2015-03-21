﻿using System;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR.Tokenized
{
    public class NextInterfaceBytesIndicator:ObservableObject
    {
        private bool taExists;
        private bool tbExists;
        private bool tcExists;
        private bool tdExists;
        private InterfaceByteGroupType groupType;

        public NextInterfaceBytesIndicator(byte indicatorBitmap, bool isT0)
        {
            this.TaExists = (indicatorBitmap & 0x10) != 0;
            this.TbExists = (indicatorBitmap & 0x20) != 0;
            this.TcExists = (indicatorBitmap & 0x40) != 0;
            this.TdExists = (indicatorBitmap & 0x80) != 0;

            this.groupType = isT0?InterfaceByteGroupType.Global : (InterfaceByteGroupType) (indicatorBitmap & 0x0F);
        }

        public NextInterfaceBytesIndicator(InterfaceByteGroupType groupType)
        {
            this.groupType = groupType;
        }

        public bool TaExists
        {
            get { return this.taExists; }
            internal set
            {
                this.SetAndInvoke(()=>TaExists, ref this.taExists, value);
                this.InvokePropertyChanged(()=>SomeBytesExist);
            }
        }

        public bool TbExists
        {
            get { return this.tbExists; }
            internal set
            {
                this.SetAndInvoke(() => TbExists, ref this.tbExists, value);
                this.InvokePropertyChanged(() => SomeBytesExist);
            }
        }

        public bool TcExists
        {
            get { return this.tcExists; }
            internal set
            {
                this.SetAndInvoke(() => TcExists, ref this.tcExists, value);
                this.InvokePropertyChanged(() => SomeBytesExist);
            }
        }

        public bool TdExists
        {
            get { return this.tdExists; }
            internal set
            {
                this.SetAndInvoke(() => TdExists, ref this.tdExists, value);
                this.InvokePropertyChanged(() => SomeBytesExist);
            }
        }

        public bool SomeBytesExist
        {
            get
            {
                return this.TaExists || this.TbExists || this.TcExists || this.TdExists;
            }
        }

        public InterfaceByteGroupType GroupType
        {
            get { return this.groupType; }
            set
            {
                if (this.groupType == InterfaceByteGroupType.Global)
                {
                    throw new InvalidOperationException("Type of global interface bytes group cannot be changed");
                }
                this.SetAndInvoke(()=>GroupType, ref this.groupType, value);
                
            }
        }

        public byte GetAsByte(byte lowerNibble)
        {
            DbC.Assure((lowerNibble & 0xF0) == 0x00);
            return (byte) (lowerNibble | (TaExists ? 0x10 : 0x00) | (TbExists ? 0x20 : 0x00) | (TcExists ? 0x40 : 0x00) | (TdExists ? 0x80 : 0x00));
        }
    }
}