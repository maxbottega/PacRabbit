/*
EasyScale Script v.1.0
Copyright © 2012 Unluck Software - Egil A Larsen
www.chemicalbliss.com
*/
#pragma strict
@script ExecuteInEditMode()
var multiplier:float = 1;
var _startSize:float =0;
var _gravityModifier:float=0;
var _startSpeed:float=0;

function OnDrawGizmosSelected () {
	if(Application.isEditor){
		if(_startSize ==0){
			_startSize = transform.GetComponent.<ParticleSystem>().startSize;
			_gravityModifier = transform.GetComponent.<ParticleSystem>().gravityModifier;
			_startSpeed = transform.GetComponent.<ParticleSystem>().startSpeed;
		}else{
		transform.GetComponent.<ParticleSystem>().startSize = _startSize * multiplier;
		transform.GetComponent.<ParticleSystem>().gravityModifier = _gravityModifier * multiplier;
		transform.GetComponent.<ParticleSystem>().startSpeed = _startSpeed * multiplier;
		}
	}
}