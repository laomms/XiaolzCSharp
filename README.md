# XiaolzCSharp
小栗子C# SDK


转一个C#调用非托管程序注意事项：  

1>C#值类型与引用类型的内存特点  
2>平台调用中DllImport，StructLayout，MarshalAS的各属性及其含义  
3>C++中结构体的内存布局规则  
4>C#调用非托管代码时，各种参数的送封特点(主要是结构体，数组，字符串)  
5>使用Marshal类的静态方法实现托管内存与非托管内存之间的转换  
6>内存释放问题，即C#中如何释放非托管代码申请的内存  

1>C#值类型与引用类型的内存特点  
C#值类型的对象是在堆栈上分配的，不受垃圾回收器的影响。  
C#引用类型的对象是在托管堆上分配的，对应的引用地址会存储在线程的堆栈区。  
值类型包括C#的基本类型（用关键字int、char、float等来声明），结构（用struct关键字声明的类型），枚举（用enum关键字声明的类型）；引用类型包括类（用class关键字声明的类型），委托（用delegate关键字声明的特殊类）和数组。  

2>平台调用中DllImport，StructLayout，MarshalAS的各字段及其含义  
DllImport属性：  
dllName：必须字段，指定对应的dll的路径，可以使用绝对路径也可以使用相对路径。如果使用相对路径，则系统会在以下三个文件夹下寻找该相对路径：1，exe所在文件夹；2，系统文件夹；3，Path环境变量指定的文件夹。  
EntryPoint：指定要调用的DLL入口点。注意如果使用extern"C"+stdcall则编译器会对函数名进行重整。最终的函数名会是_FuncName@，8其中8为FuncName函数所有参数的字节数。如果是extern"C"+cdecl调用约定，则函数名不变。  
举例：函数声明如下：  
```C
  extern "C"
  {
   __declspec(dllexport) int __stdcall foo(long bar);
  }
```
对应的函数入口点如下：  
```C
  int __stdcall foo(long bar)
  {
    return 0;
  }
```
注：  
其中extern"C"使得C++编译器生成的函数名对于C编译器是能够理解的，因为C++编译器为了处理函数重载的情况，将参数类型加入到了函数的签名中，所以生成的函数入口只能C++编译器自己懂。而加入extern"C"则使得生成的函数签名能够被其他编译器理解。  
stdcall和cdecl是两种调用约定方式。主要区别在于压入堆栈中的函数参数的清理方式，stdcall规定被调用函数负责参数出栈，称自动清除；  
cdecl则规定调用函数方负责参数出栈，称手动清除。编译器一般默认使用cdel方式。  
CharSet：  
控制函数中与字符串有关的参数或结构体参数中与字符串有关的参数在封送时的编码方式。编码方式有两种：ANSI和UNICOD。EANSI使用1个字节对前256个字符进行编码，而UNICODE使用两个字节对所有字符进行编码。.Net平台中使用的是Unicode格式。在C++中可以使用多种字符集。  
从托管代码中传递字符串到非托管代码中时，如果非托管代码使用的是ANSI，则需指定封送方式为Charset.Ansi，封送拆收器会根据该设置将Unicode字符转换为ANSI字符，再复制到非托管内存中，如果非托管代码使用Unicode，则需要指定Charset.Unicode，封送拆收器则直接复制过去，在效率上会好一些。  
MarshalAS属性：用来指定单个参数或者是结构体中单个字段的封送方式。该属性有以下字段：  
UnmanagedType：必须字段。用于指定该参数对应非托管数据类型。由于C#中的数据类型和C++中的数据类型不是一一对应的，有些时候C#中的同一种数据类型可以对应于C++中的几种数据类型，所以需要指定该参数，封送拆收器会在两个类型之间进行相应的类型转换。
比如C#中的String类型，则可以对应于非托管C++中的char*或者wchat_t*。如果是char*，则指定UnmanagedType.LPStr，如果是wchat_t*，则指定为UnmanagedType.LPWStr。
另一个例子是C#中的托管类型System.Boolean可以对应非托管C++中的bool，但是C++中的bool可能是1个字节，2个字节或者4个字节。这时就需要指定为UnmanagedType.U1,UnmanagedType.U2或者UnmanagedType.U4。  
本项目中KXTV_BOOLEA为N1字节无符号数：typedef unsignedchar KXTV_BOOLEAN;所以在C#中：  
```C
using KXTV_BOOLEAN=System.Boolean;
[MarshalAs(UnmanagedType.U1)]
KXTV_BOOLEAN NetUserFlag,
```
count：对于需要传递定长字符串或者数组的情况，需要使用count字段来指定字符串长度或者数组中元素的个数。  
StructLayout属性：控制C#中的结构体在内存中的布局。为了能够和非托管C++中的结构体在内存中进行转换，封送拆收器必须知道结构体中每一个字段在结构体中内存中的偏移量。
LayoutKind：指定结构体的布局类型。   
有两种布局类型可以设置，1，Sequential：顺序布局。2，Explicit：精确布局。可以精确控制结构体中每个字段在非托管内存中的精确位置。  
一般使用顺序布局方式，这也是C#默认的布局方式。   
但是在以下两种情况下需要使用精确控制Explicit方式：  
1，部分定义结构体中的字段。有些结构体很庞大，而C#中仅使用其中几个字段，则可以只定义那几个字段，但是要精确指定它们在非托管内存中精确偏移。该偏移应与有其他字段时的偏移一致。  
2，非托管代码中的联合体，需要使用Explicit将字段重合在一起。如： 
```C
       [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct KXTV_VALUE
        {
            [FieldOffset(0)]
            public KXTV_UINT16DataType;///数据类型(KDB_VALUE_DATA_TPYE)
            [FieldOffset(2)]
            [MarshalAs(UnmanagedType.U1)]
            public KXTV_BOOLEAN bitVal;///布尔类型
            [FieldOffset(2)]
            public KXTV_INT8i 1Val;///单字节整数oy
            [FieldOffset(2)]
            public KXTV_INT16i 2Val;///双字节整数oy
            [FieldOffset(2)]
            public KXTV_INT32i 4Val;///四字节整数oy
            [FieldOffset(2)]
            public KXTV_INT64i 8Val;///八字节整数oy
            [FieldOffset(2)]
            public KXTV_UINT8ui 1Val;///单字节整数oy(无符号)
            [FieldOffset(2)]
            public KXTV_UINT16ui 2Val;//双字节整数(无
            [FieldOffset(2)]
            public KXTV_UINT32ui 4Val;//四字节整数(无
            [FieldOffset(2)]
            public KXTV_UINT64ui 8Val;///八字节整数(无符号)
            [FieldOffset(2)]
            public KXTV_FLOAT32r 4Val;///单精度浮点
            [FieldOffset(2)]
            public KXTV_FLOAT64r 8Val;///双精度浮点数///
            [FieldOffset(2)]
            public KXTV_PTR refVal;///其他类型
        };
```
但是需要注意的是值类型和引用类型的地址不能够重叠。所以上面使用refVal代表所有引用类型，最后通过Marshal类进行转换即可。  

3>C++中结构体的内存布局规则  
结构体中字段的偏移量受两个因素的影响，一个是字段本身在内存中的大小，另一个是对齐方式。  
字段的偏移量为min（字段大小的倍数，对齐方式的倍数），且要保证字段不能重叠，即偏移量应该大于上一个字段的结尾。  
对齐方式默认为8，也可以通过pack来设置新的对齐方式，在KvTXAPI.h中，对齐方式设置为：  
#pragmapack(1)  
即对齐方式设置为1。这时字段在内存中是连续分布的，字段与字段之间没有空隙。  
对应于C#中，所有的结构体都必须使用StructLayout属性中的Pack字段来指定对齐方式。  
如：  
```C
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct KXTVTagPubData
        {
            //[FieldOffset(0)]
            public KXTV_UINT32 TagID;
            //[FieldOffset(4)]
            public KXTV_INT16 FieldID;
            //[FieldOffset(6)]
            public KXTV_VALUE FieldValue;
            //[FieldOffset(16)]
            public FILETIME TimeStamp;
            //[FieldOffset(24)]
            public KXTV_UINT32 QualityStamp;
        };
```

如果指定了Pack=1，则FieldValue字段的偏移是6，如果不指定的话，由于FieldValue本身字段大小为10字节，默认对齐方式为8，前面字段已经使用了6个字节了，所以FieldValue的偏移是8（计算公式为min（10*1，8*1））。   
结构体在内存中的大小不一致，会导致封送处理器复制内存时出错。   

4>C#调用非托管代码时，各种参数的送封特点(主要是结构体，数组，字符串)  
结构体：  
一般使用引用传递结构体参数。结构体在C#中本身是值类型，一般在线程的堆栈区分配内存。
例子：
```C
    KXTV_RETKXTVAPIKXTVServerConnect(INPKXTV_CONNECTION_OPTION pConnectOption,OUTKXTV_HANDLE* ServerHandle);  
```
其中参数pConnectOption是一个结构体指针类型变量。由于参数是指针类型则可以使用引用传递方式传递结构体，在C#中方法的声明为：
```C
[DllImport(dllName,CallingConvention=CallingConvention.StdCall,CharSet=CharSet.Unicode)]
publicexternstaticKXTV_RETKXTVServerConnect(refKXTVConnection OptionConnectOption,ref KXTV_HANDLE ServerHandle);
```
在C#中的调用方式为：
```C
KXTV_RETErrorCode;
KXTV.KXTVConnectionOptionConnectOption=newKXTV.KXTVConnectionOption();
ConnectOption.ServerName="127.0.0.1";
ConnectOption.ServerPort="8800";
ConnectOption.UserName="KVAdministrator";
ConnectOption.Password="KVADMINISTRATOR@KING";
ConnectOption.ConnectionFlags=2;
ConnectOption.NetUserFlag=false;
ConnectOption.NetworkTimeout=0;
ConnectOption.Reserved1=1;
ErrorCode=KXTV.KXTVServerConnect(ref ConnectOption,ref m_hClient);
if(IsOK(ErrorCode,"KXTVServerConnect")==false)
    return false;
```
其中ConnectOption为局部变量，在托管内存的堆栈区分配的空间。函数参数的传递方式为引用传递，并不是将堆栈区ConnectOption的地址传递给非托管函数，而是由封送拆收器根据结构体的定义在非托管空间开辟一块内存，将堆栈区的ConnectOption的各字段按照指定的方式封送过去，再取得该内存地址封送给非托管函数，非托管函数完成操作后，封送拆收器会将数据按照指定的方式封送回来，最后由封送拆收器释放掉非托管去的内
存。所以结构体中使用StructLayout属性来控制字段在内存中的布局很重要，如果和C++编译器生成的内存布局不一致，则会导致无法取得结构体中的某些变量，从而导致程序出错。  

数组：  
数组是引用类型，对于引用类型按照值传递方式传送过去的也是地址。如果数组中的元素是简单的blittable类型（该类型变量在C#和C++中的内存结构是一致的，可直接复制到本机代码（nativecode）的类型）。则会将数组的指针直接传送过去，这是C#的优化，没有值拷贝的过程。
blittable类型有：Byte；SByte；Int16,；Uint16；Int32；Uint32；Int64；Uint64；Single；Double；IntPtr；UIntPtr。  
blittable类型基本上都是整型和实数型，这些类型在托管内存和非托管内存中的大小都是一致的，可以不通过任何转换进行互操作。  
而对于所有的字符类型，接口，类，结构体等，都需要封送拆收器进行一些转换，所以都会在非托管内存中复制一份拷贝。而blittable类型则不用。
所以使用blittable类型的数组作为值传递过去，在非托管函数中的任何修改都会在C#中可见的。如果这不是需要的，则必须指定参数传递的方向属性为[in]，即不返回修改的值，这时封送拆收器就进行值复制的过程。 

字符串：  
字符串的封送过程要注意字符集的问题。C#中使用的Unicode编码，而C++中的则不一定，所以需要CharSet进行修饰String，这样封送拆收器就能进行相应的转换。
C#中的String的值是不可原地修改的，而System.Text.StringBuilder的值是可以原地修改的。在方向属性上StringBuilder对象比较特殊，在没有标注参数的方向属性是，封送拆收器采用[In]的方式进行默认的处理，对于StringBuilder，封送拆收器则采用[In,Out]方式进行封送处理，即如果StringBuilder传递过去的字符串发生了修改，封送拆收器默认会将修改的值再封送回StringBuilder对象中。  

5>使用Marshal类的静态方法实现托管内存与非托管内存之间的转换  
对于C++中的一些复杂类型在C#中没有相应的数据类型与之对应，所以只能使用指针IntPtr来获取该对象在托管内存中的地址，然后使用Marshal类提供的静态方法将非托管内存中的对象复制到托管内存中。
本项目中：
```C
typedef struct KXTVStringArray
{
    KXTV_UINT32 SizeOfArray;///数组大小
    KXTV_WSTR_ARRAY StringArray;///字符串数组
}KXTV_STRING_ARRAY, * PKXTV_STRING_ARRAY;
```
该结构体的StringArray为指向指针数组的指针，该指针数组中的每个元素指向一个字符串。这些字符串的内存大多在C++非托管内存中申请的。需要使用IntPtr来取代StringArray。
然后用Marshal中的方法解析出字符串数组：
```C
        public static KXTV_WSTR[] parseToString(KXTV.KXTVStringArray TagNameArray)
        {
            uintsize = TagNameArray.SizeOfArray;
            KXTV_PTR[] ptrs = newKXTV_HANDLE[size];
            Marshal.Copy(TagNameArray.StringArray, ptrs, 0, (int)size);//将指针数组复制到ptrs指向的内存中
            KXTV_WSTR[] str = new KXTV_WSTR[size]; for (uinti = 0; i < size; i++)
            {
                str[i] = Marshal.PtrToStringUni(ptrs[i]);//将非托管内存中的字符串复制并构建托管内存中的String对象
            }
            returnstr;
        }
```
对于指针转换为结构体，Marshal类也提供了PtrToStructure方法，本项目中的应用为： 
```C
KXTV.KXTVTagPubDataTagPubData=(KXTV.KXTVTagPubData)Marshal.PtrToStructure(pFieldValueArray,typeof(KXTV.KXTVTagPubData));
```
之后非托管中的内存需要调用对应的函数进行释放。   

6>内存释放问题，即C#中如何释放非托管代码申请的内存   
非托管C++代码中分配内存有三种方式，malloc，new，CoTaskMenAlloc。对于前两种方式需要在非托管内存中释放内存，第三种方式可以在非托管内存中释放，也可以在托管内存中释放。  
如果C#调用的函数有一个指针参数，该指针参数在C++中分配了内存，并传递回来了，这时就需要在使用完这块内存之后释放掉，否则会出现内存泄漏。  
在本项目中，也有C++分配内存的例子，如  
```C
KXTV_RETKXTVAPI KXTVTagGetTagNamebyID(INKXTV_HANDLE ServerHandle, INKXTV_UINT32 TagNum, INKXTV_UINT32* TagIDArray, OUTPKXTV_STRING_ARRAY TagNameArray, OUTKXTV_RET* ErrorCodeArray);
```
TagNameArray的内存是在KXTVTagGetTagNamebyID函数内部分配的，由于不知道内存的分配方式，所以只能使用运行库提供的接口来释放这部分内存。释放内存的接口为：
```C
KXTV_RETKXTVAPIKXTVAPI FreeStringArray(INPKXTV_STRING_ARRAY StringArray);
``` 
这里主要讨论一下在托管C++中使用COM的内存分配方法CoTaskMenAlloc的情况。在这种情况下可以在C#中释放非托管内存。
如有如下函数：
```C
boolMallocString(chat*pStr);
``` 
该函数使用CoTaskMenAlloc分配了一段内存给pStr后返回。该函数在C#中对应的声明可写为：
```C
[DllImport("dllName.dll",CharSet=CharSet.Ansi)]
boolMallocString([Out]StirngStr);
``` 
函数返回时，封送拆收器在托管内存中建立Str对象，并将非托管内存中的字符串拷贝到托管内存Str对象中，完成转换后，封送拆收器会调用CoTaskMenFree尝试释放pStr指向的非托管内存，如果该内存是由CoTaskMenAlloc方式分配的，则释放成功。如果是用new或malloc申请的，则释放失败，这时就出现了内存泄漏的情况。  
所有封送拆收器在进行类型转换时，会自动调用CoTaskMenFree方法来尝试释放内存，当然也可以显示释放内存。  
使用IntPtr数据类型来对应pStr，封送拆收器将非托管数据封送成IntPtr时，直接将指针复制进IntPtr的值中，如上声明为：
```C
[DllImport("dllName.dll",CharSet=CharSet.Ansi)]
bool MallocString([Out] IntPtr pStr);
``` 
使用Marshal类的PtrToStringAni()方法实现数据拷贝:
```C
String Str=Marshal.PtrToStringAni(pStr);
``` 
这时数据已经复制到Str对象中了，可以释放非托管内存中pStr的内存了：
```C
Marshal.FreeCoTaskMem(pStr);
``` 
这就是手动释放CoTaskMenAlloc分配的内存，当然也可以在C++中提供一个函数来释放分配的内存：
```C
bool ReleaseString(char*pStr);
``` 
之后在C#中调用该函数释放内存也可以。注：对于不确定内存分配方式的，只能使用C++提供的函数来释放内存。  
本项目中的例子：
```C
KXTV.KXTVTagGetTagNamebyID(m_hClient,1,TagIDArray,ref TagNameArray,ErrorCodeArray);
String[] TagNames=parseToString(TagNameArray);
KXTV.KXTVAPIFreeStringArray(ref TagNameArray);/释放内存
``` 
调用Marshal.AllocHGlobal必须调用 Marshal.FreeHGlobal(ptr)来手动释放内存，即使调用GC.Collect();方法也无法释放，导致内存泄露！！  

6>略谈unsafe   
msdn里讲到:
“在 C# 中很少需要使用指针，但仍有一些需要使用的情况。例如，在下列情况中使用允许采用指针的不安全上下文是正确的：    
处理磁盘上的现有结构
涉及内部包含指针的结构的高级 COM 或平台调用方案
性能关键代码
对于第一和第二点，主要是调win32的api。
但是“性能关键代码”这个非常重要。我来举例引申一下。  
我们都知道像飞信这种大型IM服务端，难免会面临大量的字符处理（协议报文）。
如果同时在线100多万，而且大家都同时进行会话，服务端的程序如果对内存回收不好，那肯定会crash.
飞信服务端是.net的，所以就拉扯一下这个例子。
不过你可以大胆猜测，它服务端肯定用了unsafe，不然顶不住这档子活!!
还是msdn上的demo:
以下示例使用指针将一个字节数组从 src 复制到 dst。用 /unsafe 选项编译此示例.
```C
    class Test
    {
        // The unsafe keyword allows pointers to be used within
        // the following method:
        static unsafe void Copy(byte[] src, int srcIndex,
        byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
            dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            int srcLen = src.Length;
            int dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
            dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }
            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.
            fixed (byte* pSrc = src, pDst = dst)
            {
                byte* ps = pSrc;
                byte* pd = pDst;
                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (int n = 0; n < count / 4; n++)
                {
                    *((int*)pd) = *((int*)ps);
                    pd += 4;
                    ps += 4;
                }
                // Complete the copy by moving any bytes that weren’t
                // moved in blocks of 4:
                for (int n = 0; n < count % 4; n++)
                {
                    *pd = *ps;
                    pd++;
                    ps++;
                }
            }
        }
        static void Main(string[] args)
        {
            byte[] a = new byte[100];
            byte[] b = new byte[100];
            for (int i = 0; i < 100; ++i)
                a[i] = (byte)i;
            Copy(a, 0, b, 0, 100);
            Console.WriteLine(”The first 10 elements are:”);
            for (int i = 0; i < 10; ++i)
                Console.Write(b[i] + ” “);
            Console.WriteLine(”/ n”);
        }
    }
``` 
请注意使用了 unsafe 关键字，这允许在 Copy 方法内使用指针。  
fixed 语句用于声明指向源和目标数组的指针。它锁定 src 和 dst 对象在内存中的位置以便使其不会被垃圾回收移动。当 fixed 块完成后，这些对象将被解除锁定。
通过略过数组界限检查，不安全代码可提高性能。  
fixed 语句允许您获取指向字节数组使用的内存的指针，并且标记实例，以便垃圾回收器不会移动它。  
在 fixed 块的末尾，将标记该实例以便可以移动它。此功能称为声明式锁定。锁定的好处是系统开销非常小，除非在 fixed 块中发生垃圾回收（但此情况不太可能发生）。  
对头，fixed 内我只分配我自己的内存，用完就释放，从不霸占平民土地，不多征收平民余粮！！  
对于如果你要是等着GC来跟你处理，它寻根寻址还得点时候呢。。。。。  

7>略谈GCHandle   
我们在使用c#托管代码时，内存地址和GC回收那不是我们关心的，CLR已经给我们暗箱操作。
但是如果我们在c#中调用了一个非托管代码，比如vc的DLL,而且他有个回调函数，需要引用c#中的某个对象并操作，
这时候你就得要小心了。
要是非托管代码中用到得托管代码那个对象被GC给回收了，这时候就会报内存错误。
所以我们就要把那个对象“钉”住(pin)，让它的内存地址固定，而不被垃圾回收掉，然后最后我们自己管理，自己释放内存,这时候就需要GCHandle,来看个msdn上的例子:  
```C
public delegate bool CallBack(int handle, IntPtr param);
        public class LibWrap
        {
            [DllImport("user32.dll")]
            public static extern bool EnumWindows(CallBack cb, IntPtr param);
        }

        class Program
        {
            static void Main(string[] args)
            {

                TextWriter tw = System.Console.Out;
                GCHandle gch = GCHandle.Alloc(tw);
                CallBack cewp = new CallBack(CaptureEnumWindowsProc);
                LibWrap.EnumWindows(cewp, (IntPtr)gch);
                gch.Free();
                Console.Read();

            }
            private static bool CaptureEnumWindowsProc(int handle, IntPtr param)
            {
                GCHandle gch = (GCHandle)param;
                TextWriter tw = (TextWriter)gch.Target;

                tw.WriteLine(handle);
                return true;
            }

        }
``` 
对上面的代码，略加解释：gch 会钉住(pin)tw这个对象，使其不受GC管理，告诉它，以后你崩管我，我也不用给你上税,其实管理权已经给gch，通过free来释放内存。
这种情况主要用在托管和非托管代码交互的时候，防止内存泄露来使用GCHandle。  
另也可以使用GC.KeepAlive 方法(引用msdn)  
KeepAlive 方法的目的是确保对对象的引用存在，该对象有被垃圾回收器过早回收的危险。这种现象可能发生的一种常见情形是，当在托管代码或数据中已没有对该对象的引用，但该对象仍然在非托管代码（如 Win32 API、非托管 DLL 或使用 COM 的方法）中使用。
下面是例子:
```C
using System;
using System.Threading;
using System.Runtime.InteropServices;

// A simple class that exposes two static Win32 functions.
// One is a delegate type and the other is an enumerated type.
public class MyWin32
{
    // Declare the SetConsoleCtrlHandler function 
    // as external and receiving a delegate.   
    [DllImport("Kernel32")]
    public static extern Boolean SetConsoleCtrlHandler(HandlerRoutine Handler,
        Boolean Add);

    // A delegate type to be used as the handler routine 
    // for SetConsoleCtrlHandler.
    public delegate Boolean HandlerRoutine(CtrlTypes CtrlType);

    // An enumerated type for the control messages 
    // sent to the handler routine.
    public enum CtrlTypes
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }
}

public class MyApp
{
    // A private static handler function in the MyApp class.
    static Boolean Handler(MyWin32.CtrlTypes CtrlType)
    {
        String message = "This message should never be seen!";

        // A switch to handle the event type.
        switch (CtrlType)
        {
            case MyWin32.CtrlTypes.CTRL_C_EVENT:
                message = "A CTRL_C_EVENT was raised by the user.";
                break;
            case MyWin32.CtrlTypes.CTRL_BREAK_EVENT:
                message = "A CTRL_BREAK_EVENT was raised by the user.";
                break;
            case MyWin32.CtrlTypes.CTRL_CLOSE_EVENT:
                message = "A CTRL_CLOSE_EVENT was raised by the user.";
                break;
            case MyWin32.CtrlTypes.CTRL_LOGOFF_EVENT:
                message = "A CTRL_LOGOFF_EVENT was raised by the user.";
                break;
            case MyWin32.CtrlTypes.CTRL_SHUTDOWN_EVENT:
                message = "A CTRL_SHUTDOWN_EVENT was raised by the user.";
                break;
        }

        // Use interop to display a message for the type of event.
        Console.WriteLine(message);

        return true;
    }

    public static void Main()
    {

        // Use interop to set a console control handler.
        MyWin32.HandlerRoutine hr = new MyWin32.HandlerRoutine(Handler);
        MyWin32.SetConsoleCtrlHandler(hr, true);

        // Give the user some time to raise a few events.
        Console.WriteLine("Waiting 30 seconds for console ctrl events");

        // The object hr is not referred to again.
        // The garbage collector can detect that the object has no
        // more managed references and might clean it up here while
        // the unmanaged SetConsoleCtrlHandler method is still using it.      

        // Force a garbage collection to demonstrate how the hr
        // object will be handled.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Thread.Sleep(10000);

        // Display a message to the console when the unmanaged method
        // has finished its work.
        Console.WriteLine("Finished!");

        // Call GC.KeepAlive(hr) at this point to maintain a reference to hr. 
        // This will prevent the garbage collector from collecting the 
        // object during the execution of the SetConsoleCtrlHandler method.
        GC.KeepAlive(hr);
        Console.Read();
    }
}

``` 

