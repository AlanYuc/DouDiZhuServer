using System;
using System.Collections;
using System.Collections.Generic;

public class ByteArray
{
    /// <summary>
    /// Ĭ�ϵ��ֽ������С
    /// </summary>
    const int DEFAULT_SIZE = 1024;
    /// <summary>
    /// ��ʼ��С
    /// </summary>
    private int initSize;
    /// <summary>
    /// �ֽ�����
    /// </summary>
    public byte[] bytes;
    /// <summary>
    /// ����λ��
    /// </summary>
    public int readIndex;
    /// <summary>
    /// д��λ��
    /// </summary>
    public int writeIndex;
    /// <summary>
    /// �ֽ����������
    /// </summary>
    public int capacity;
    /// <summary>
    /// ��ǰ��ȡ�����ݳ���
    /// </summary>
    public int Length { get { return writeIndex - readIndex; } }
    /// <summary>
    /// ʣ�����ݵĳ���
    /// </summary>
    public int Remain { get { return capacity - writeIndex; } }

    /// <summary>
    /// �ṩ�ֽ����鳤�ȵĹ��캯��
    /// </summary>
    /// <param name="size">���鳤��</param>
    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        initSize = size;
        readIndex = 0;
        writeIndex = 0;
        capacity = size;
    }
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        initSize = defaultBytes.Length;
        readIndex = 0;
        writeIndex = defaultBytes.Length;
        capacity = defaultBytes.Length;
    }

    /// <summary>
    /// �ƶ�����
    /// </summary>
    public void MoveBytes()
    {
        if(Length > 0)
        {
            Array.Copy(bytes, readIndex, bytes, 0, Length);
        }
        writeIndex = Length;
        readIndex = 0;
    }

    /// <summary>
    /// ���������С
    /// </summary>
    /// <param name="size">�µĴ�С</param>
    public void Resize(int size)
    {
        //�������Ĵ�СС�ڵ�ǰ���ݳ���(Length)��ֱ�ӷ��ز����κβ���
        //�������ݽضϵķ���
        if (size < Length)
        {
            return;
        }
        //�������Ĵ�СС�ڳ�ʼ��С(initSize)��Ҳֱ�ӷ���
        //����Ƶ�������С������
        if (size < initSize)
        {
            return;
        }
        capacity = size;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIndex, newBytes, 0, Length);
        bytes = newBytes;
        writeIndex = Length;
        readIndex = 0;
    }
}
