using UnityEngine;
using System.Collections;
using thelab.mvc;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainButtonController :Controller<SdlApplication>  {
	public GameObject lastButton;
    private GameObject currentButton;
	private Transform diTrans;
	MenuShowComponent[] levelTwos;
	// Use this for initialization
	void Awake () {
		Transform buttonRoot = GameObject.Find ("MainButtons").transform;
		levelTwos = buttonRoot.GetComponentsInChildren<MenuShowComponent> ();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void OnNotification(string p_event,Object p_target,params object[] p_data){
		switch (p_event) {
		
		case "di@click":
			initLevelTwos();
			ButtonView di=(ButtonView)p_target;
			di.transform.localScale=Vector3.zero;
			break;
		case "ButtonLevel1@click":
			Log("click");
			initLevelTwos();
			ButtonView b=(ButtonView)p_target;
			b.transform.parent.FindChild("Di").localScale=Vector3.one;
			if (b.transform.Find("MenuLevel2").localScale==new Vector3(1,1,1)) {
				b.GetComponentInChildren<MenuShowComponent>().Hide();
			}else{
				b.GetComponentInChildren<MenuShowComponent>().Show();
			}	
			break;
		}
	}

	/// <summary>
	/// init LevelTwoButtons
	/// </summary>
	void initLevelTwos(){
	for (int i = 0; i <levelTwos.Length ; i++) {
			levelTwos[i].Hide();
		}
	}
}
