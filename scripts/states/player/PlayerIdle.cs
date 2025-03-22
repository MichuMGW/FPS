// using Godot;
// using System;

// public partial class PlayerIdle : State
// {
// 	SpellCastManager spm;
//     public override void Enter()
//     {
//         //TODO: Animacja domyślna
// 		spm = GetParent() as SpellCastManager;
//     }

//     public override void Exit()
//     {
//         return;
//     }

//     public override void Update(double delta)
//     {
//         if(Input.IsActionJustPressed("NextSpell")){
//             if(spm.spells.Count - 1 == spm.currentSpell){
//                 spm.currentSpell = 0;
//             }
//             else {
//                 spm.currentSpell++;
//             }
//         }
//         if(Input.IsActionJustPressed("PreviousSpell")){
//             if(spm.currentSpell == 0){
//                 spm.currentSpell = spm.spells.Count - 1;
//             }
//             else {
//                 spm.currentSpell--;
//             }
//         }
// 		if (Input.IsActionJustPressed("CastSpell")){
// 			EmitSignal(SignalName.Transitioned, this, "PlayerCast");
// 		}
//     }

//     public override void Physics_Update(double delta)
//     {
//         return;
//     }

// }
