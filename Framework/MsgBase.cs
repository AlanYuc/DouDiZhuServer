using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
#nullable disable
public class MsgBase
{
    /// <summary>
    /// ��ͬ��Э����
    /// </summary>
    public string protocolName;
    /// <summary>
    /// ��MsgBase�����Ϊjson���ݵ��ֽ�����
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonConvert.SerializeObject(msgBase);
        return Encoding.UTF8.GetBytes(s);
    }
    /// <summary>
    /// ���������Ϣ�ֽ��������ΪMsgBase����
    /// </summary>
    /// <param name="protocolName">Э����</param>
    /// <param name="bytes">�ֽ�����</param>
    /// <param name="offset">��ʼ�������ʼλ��</param>
    /// <param name="length">��Ҫ��������ݳ���</param>
    /// <returns></returns>
    public static MsgBase Decode(string protocolName, byte[] bytes, int offset, int count)
    {
        string s = Encoding.UTF8.GetString(bytes, offset, count);
        //return JsonUtility.FromJson<MsgBase>(s);
        //����Э�������䷴���л�Ϊ��Ӧ�����࣬Ȼ��ת��MsgBase���ࡣ
        //return JsonUtility.FromJson(s,Type.GetType(protocolName)) as MsgBase;
        return JsonConvert.DeserializeObject(s, Type.GetType(protocolName)) as MsgBase;
    }
    /// <summary>
    /// ����Э����
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeProtocolName(MsgBase msgBase)
    {
        //���л��ַ�����ʱ����Ҫ֪�����ַ������л���ĳ��ȣ����㷴���л���ʱ��ȡ����
        int indexNum = sizeof(int) + Encoding.UTF8.GetBytes(msgBase.protocolName).Length;
        byte[] bytes = new byte[indexNum];
        int index = 0;//��bytes�����еĵڼ���λ��ȥ�洢����
        byte[] nameByte = Encoding.UTF8.GetBytes(msgBase.protocolName);
        int nameNum = nameByte.Length;
        BitConverter.GetBytes(nameNum).CopyTo(bytes, index);
        index += sizeof(int);
        nameByte.CopyTo(bytes, index);
        index += nameNum;
        return bytes;
    }
    /// <summary>
    /// ����Э����
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string DecodeProtocolName(byte[] bytes, int offset, out int count)
    {
        count = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        string s = Encoding.UTF8.GetString(bytes, offset, count);
        //count����Ҫ������Ϣ���ĳ��ȣ���Ҫ����֮ǰ��int�ĳ��ȣ���Ϊ�ܵ����ݳ���
        count += sizeof(int);
        return s;
    }
}
