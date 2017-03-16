using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OpcCommander
{
    public class MyVariant
    {
        public MyVariant()
        {
            this.set(null,VarEnum.VT_EMPTY);
        }

        public MyVariant(object o, VarEnum type)
        {
            this.set(o,type);
        }

        public Object GetVar()
        {
            return this.value;
        }

        public void SetValue(string str, VarEnum t)
        {
            this.type = t;
            try
            {
                switch(t)
	            {
		            case VarEnum.VT_EMPTY:
                            this.value = 0;
					        break;
		            case VarEnum.VT_BOOL:
					        value= Convert.ToBoolean(str);
					        break;
                    case VarEnum.VT_UI1:
					        value = Convert.ToByte(str);
					        break;
                    case VarEnum.VT_I1:
					        value = Convert.ToChar(str);
					        break;
                    case VarEnum.VT_I2:
					        value = Convert.ToInt16(str);
					        break;
                    case VarEnum.VT_UI4:
					        value = Convert.ToInt32(str);
					        break;
                    case VarEnum.VT_I4:
					        value =Convert.ToInt32(str);
					        break;
                    case VarEnum.VT_DECIMAL:
                            value = Convert.ToDecimal(str);
                            break;
                    case VarEnum.VT_R4:
					        value = Convert.ToSingle(str);
					        break;
                    case VarEnum.VT_R8:
					        value =Convert.ToDouble(str);
					        break;
                            //if variant= 
                    case VarEnum.VT_VARIANT:
                            value = new MyVariant(str,t);
                            break;
                    case VarEnum.VT_BSTR:
                            str=str.Replace("\"", "");
                            str=str.Replace("\'", "");
                            value = str;
					        break;

                    case VarEnum.VT_UI1 | VarEnum.VT_ARRAY:
                            value = null;
					        break;

                    case VarEnum.VT_BSTR | VarEnum.VT_ARRAY:
                            value = null;
                            break;

                    case VarEnum.VT_I4 | VarEnum.VT_ARRAY:
                            value = null;
                            break;

                    case VarEnum.VT_BOOL | VarEnum.VT_ARRAY:
                            value = null;
                            break;

                    case VarEnum.VT_VARIANT | VarEnum.VT_ARRAY:
                            value = null;
                            break;

                    default:
                            {
                                this.value = null;
                                throw new Exception("Unknow type: " + t.ToString());
                            }
	            }
            }
            catch(Exception e)
            {
                throw new Exception("Bad type conversion: (" + e.Message + ")");
            }
        }

        public string GetStringType()
        {
            string ret = MyServer.VarEnumToString(this.type);     
            return ret;
        }

        public string GetStringValue()
        {
            string ret = "";
            if (value == null)
            {
                ret = "Empty";
            }
            else if (value is MyVariant)
            {
                ret = ((MyVariant)value).GetStringValue();
            }
            else if (value is Array)
            {
                ret = "Array ("+value.ToString()+")";
            }
            else
            {
                ret = value.ToString();
            }
            ret=ret.Replace('\u0007', '-');
            return ret;
        }

        //private
        private void set(object o,VarEnum t)
        {
            this.value = o;
            this.type = t;
        }
        private object value=null;
        private VarEnum type;
    }
}
