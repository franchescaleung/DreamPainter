using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dreamwork
{
	public class LevelsData : MonoBehaviour
	{
		public GameObject ladderPrefab;
		public Tile LbridgeTile, CbridgeTile, RbridgeTile; //L,C,R
		public Dictionary<string, GameObject[]> instantiations = new Dictionary<string, GameObject[]>();
		public GameManager GM;



		public string getResponse(int level_num, string action_area, string itemName)
		{

			// LEVEL 1
			if (level_num == 1)
			{
				if (action_area == "1")
				{

					//AA: (10,-1) - (11,3)

					if (itemName == "ladder")
					{
						if (instantiations.ContainsKey("AA1.1")) return "";

						instantiations.Add("AA1.1", new GameObject[5]);
						for (int i = 0; i < 5; i++)
						{
							instantiations["AA1.1"][i] = Instantiate(ladderPrefab);
							instantiations["AA1.1"][i].transform.position = new Vector3(11, 3.5f - i, 0);
						}
						// 5 ladders 32x32 [(11,3.5),(11,2.5),(11,1.5),(11,0.5),(11,-0.5)]

						GM.removeActionArea(10, -1, 11, 3);
						return "AA1.1";
					}
				}

				else if (action_area == "2")
				{
					//AA: (21,1) - (22,2)
					List<string> validItems = new List<string>(new string[] { "drill", "hammer", "broom", "cannon" });
					if (validItems.Contains(itemName))
					{
						// drill through tiles (23,1) -> (27,2)
						for (int i = 0; i < 5; i++)
						{
							GM.terrainTM.SetTile(new Vector3Int(23 + i, 1, 0), null);
							GM.terrainTM.SetTile(new Vector3Int(23 + i, 2, 0), null);
						}

						GM.removeActionArea(21, 1, 22, 2);
						return "AA1.2";
					}
				}

				else if (action_area == "3")
				{
					//AA: (-15,1) - (-8,4)
					List<string> validItems = new List<string>(new string[] { "bridge" });


					if (validItems.Contains(itemName))
					{
						GM.terrainTM.SetTile(new Vector3Int(-14, 1, 0), LbridgeTile);
						for (int x = -13; x <= -10; x++)
						{
							GM.terrainTM.SetTile(new Vector3Int(x, 1, 0), CbridgeTile);
						}
						GM.terrainTM.SetTile(new Vector3Int(-9, 1, 0), RbridgeTile);
						GM.removeActionArea(-15, 1, -8, 4);
						return "AA1.2";
					}
				}

				else if (action_area == "light_source")
				{
					List<string> validItems = new List<string>(new string[] { "sun" });
					if (validItems.Contains(itemName))
					{
						StartCoroutine(GM.fadeOut(GM.darkBG, 2));
					}
					return "let there be light";
				}

			}
			return "";
		}
	}
}