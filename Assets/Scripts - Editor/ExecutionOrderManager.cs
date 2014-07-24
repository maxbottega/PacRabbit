using UnityEditor;

[InitializeOnLoad]
public class ExecutionOrderManager : Editor
{
	static ExecutionOrderManager()
	{
		{
			string scriptName = typeof(CollisionManager).Name;
			
			foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
				if (monoScript.name == scriptName)
				{
					if (MonoImporter.GetExecutionOrder(monoScript) != 1000) // Without this we will get stuck in an infinite loop	
						MonoImporter.SetExecutionOrder(monoScript, 1000);
					break;
				}
		}
		
		{
			string scriptName = typeof(SphereTransform).Name;
			
			foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
				if (monoScript.name == scriptName)
			{
				if (MonoImporter.GetExecutionOrder(monoScript) != 999) // Without this we will get stuck in an infinite loop	
					MonoImporter.SetExecutionOrder(monoScript, 999);
				break;
			}
		}
	}
}