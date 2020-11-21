using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamwork{
public class ItemsManager : MonoBehaviour
{

	[Header("Item Manager States")]
	public GameObject equipped_item = null;
	public SpriteRenderer equipped_sr;
	public string equipped_item_name;
	public int current_item_id = 0;


	//stores items prefabs
	public Dictionary <string, GameObject> item_prefabs = new Dictionary <string, GameObject>(); 
	// each item has an ID (idx when loaded) and name (prefab name)
	public Dictionary <string, int> name_to_id = new Dictionary <string, int>();
	public Dictionary <int, string> id_to_name = new Dictionary <int, string>();




	void Start()
	{
	 	loadFromResources();
	}


	void loadFromResources(){

		// load prefabs into item_prefabs
		int id = 0;
		Object[] loadedObjs = Resources.LoadAll("Items", typeof(GameObject));
		foreach (GameObject obj in loadedObjs) 
		{    
	 		item_prefabs.Add(obj.name, (GameObject) obj);
	 		name_to_id.Add(obj.name, id);
	 		id_to_name.Add(id, obj.name);
	 		id++;
		}
	}

	public void loadItem(string itemName = "", int itemID = 0){
		if(!item_prefabs.ContainsKey(itemName)) return;

		if(itemName == "") itemName = id_to_name[itemID];

		if(equipped_item) Destroy(equipped_item);
		equipped_item = Instantiate (item_prefabs [itemName]) as GameObject;
		equipped_item_name = itemName;
		equipped_sr = equipped_item.GetComponent<SpriteRenderer>();
	}


	public void destroyCurrentItem(){
		Destroy(equipped_item);
		current_item_id = -1;
		equipped_item_name = "";
		equipped_sr = null;
		equipped_item = null;
	}


}
}
