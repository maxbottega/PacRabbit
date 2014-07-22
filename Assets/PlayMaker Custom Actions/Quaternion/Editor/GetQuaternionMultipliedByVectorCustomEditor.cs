using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;


[CustomActionEditor(typeof(GetQuaternionMultipliedByVector))]
public class GetQuaternionMultipliedByVectorCustomEditor : QuaternionCustomEditorBase
{

    public override bool OnGUI()
    {
		EditField("quaternion");
		EditField("vector3");
		EditField("result");
		
		bool changed = EditEveryFrameField();
		
		return GUI.changed || changed;
    }


}
