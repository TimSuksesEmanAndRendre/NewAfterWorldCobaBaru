using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

public class StructureCreator : BaseCreator {

	/*
	trimiti toate datele de aici, nu te bazezi ca sint inregistrate in baza
	*/

	//private string[] buildingTags = new string[noOfStructures]{"Toolhouse","Forge","Generator",
	//"Vault","Barrel","Summon","Academy","Classroom","Chessboard","Globe","Workshop" "Tatami"}; 

	void Start () {

		base.InitializeComponents ();		//this is the only class who will initiate component, since there is no need to receive thee same call from all children
		//GetBuildingsXML();				//reads Buildings.xml
		ReadStructureXML();
		UpdatePrices();//"Buildings"
		StartCoroutine("UpdateLabelStats");

		if (structureXMLTag == "Building")
			StartCoroutine("RegisterBasicEconomyValues");
		
	}

	private IEnumerator RegisterBasicEconomyValues()
	{
		yield return new WaitForSeconds (xmlLoadDelay);

		int index = 0;								//instead of xml index building 2,3 producing/storing something, we will have 0,1,2...
													//xml order: Forge Generator Vault Barrel Summon
		for (int i = 0; i < structures.Count; i++) 
		{
			bool isvalid = false;

			if (structures [i] ["ProdType"] != "None")
			{				
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].ProdType = structures [i] ["ProdType"];
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].ProdPerHour = int.Parse (structures [i] ["ProdPerHour"]);

                print(ResourceGenerator.InstanceResourceGenerator.basicEconomyValues[index].ProdType);
                print(ResourceGenerator.InstanceResourceGenerator.basicEconomyValues[index].ProdPerHour);

                isvalid = true;
			}
			if (structures [i] ["StoreType"] != "None") 
			{
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].StoreType = structures [i] ["StoreType"];//Internal, Distributed
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].StoreResource = structures [i] ["StoreResource"];
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].StoreCap = int.Parse(structures [i] ["StoreCap"]);
				isvalid = true;
			}

			if (isvalid) 
			{				
				ResourceGenerator.InstanceResourceGenerator.basicEconomyValues [index].StructureType = structures [i] ["StructureType"];
				index++;
			}
		}
	}
}
