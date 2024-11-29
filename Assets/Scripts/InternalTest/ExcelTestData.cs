using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcelTestData : IUnique
{
	public int ID { get; set; }
	public string Name { get; set; }
	public float Speed { get; set; }
	[MiniExcelLibs.Attributes.ExcelColumnName("我将带头冲锋！！！")]
	public string AHead { get; set; }
	[MiniExcelLibs.Attributes.ExcelIgnore]
	public int Dummy { get; set; }
	// ReSharper disable once IdentifierTypo
	public long AVeeeeeeeryLongProperty { get; set; }
	public short S { get; set; }
	[MiniExcelLibs.Attributes.ExcelColumnWidth(15)]
	public int Middle { get; set; }

	public ExcelTestData() { }
    public ExcelTestData(ExcelTestData other) 
    {
    	ID = other.ID;
    	Name = other.Name;
    	Speed = other.Speed;
    	AHead = other.AHead;
    	Dummy = other.Dummy;
    	AVeeeeeeeryLongProperty = other.AVeeeeeeeryLongProperty;
    	S = other.S;
    	Middle = other.Middle;
    }
}
