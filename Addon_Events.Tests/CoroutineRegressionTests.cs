// Standalone regression harness: compiles the actual postprocessors with minimal
// game/Unity stubs. This checks coroutine ownership, not Unity integration.
using System;
using System.Collections;
using System.Collections.Generic;
using Lomzie.AutomaticWorkAssignment;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnPostProcessors;
using Verse;
namespace UnityEngine {
 public class Coroutine { public IEnumerator Routine; public bool Stopped; }
 public class WaitForSeconds { public WaitForSeconds(float seconds) {} }
}
namespace RimWorld { public static class GenDate { public const int TicksPerHour=2500; } }
namespace Verse {
 public class Pawn { public bool State; }
 public static class GenTicks { public static int TicksAbs; }
 public class Root {
  public List<UnityEngine.Coroutine> Started=new List<UnityEngine.Coroutine>();
  public UnityEngine.Coroutine StartCoroutine(IEnumerator routine) {
   var c=new UnityEngine.Coroutine {Routine=routine}; Started.Add(c); routine.MoveNext(); return c;
  }
  public void StopCoroutine(UnityEngine.Coroutine c) {c.Stopped=true;}
 }
 public static class Find { public static Root Root=new Root(); }
 public static class Scribe_Deep {public static void Look<T>(ref T v,string name) {}}
 public static class Scribe_Values {public static void Look(ref float v,string name) {}}
}
namespace Lomzie.AutomaticWorkAssignment {
 public class PawnSetting {public virtual void ExposeData() {}}
 public class WorkSpecification {}
 public class ResolveWorkRequest {}
}
namespace Lomzie.AutomaticWorkAssignment.PawnConditions {
 public interface IPawnCondition {bool IsValid(Pawn p,WorkSpecification s,ResolveWorkRequest r);}
}
namespace Lomzie.AutomaticWorkAssignment.PawnPostProcessors {
 public interface IPawnPostProcessor {void PostProcess(Pawn p,WorkSpecification s,ResolveWorkRequest r);}
}
class Test {
 class Condition:IPawnCondition {public bool IsValid(Pawn p,WorkSpecification s,ResolveWorkRequest r) {return p.State;}}
 class ActionCounter:IPawnPostProcessor {
  public Dictionary<Pawn,int> Counts=new Dictionary<Pawn,int>();
  public int Count(Pawn p) {return Counts.ContainsKey(p)?Counts[p]:0;}
  public void PostProcess(Pawn p,WorkSpecification s,ResolveWorkRequest r) {Counts[p]=Count(p)+1;}
 }
 static int checks;
 static void Assert(bool ok,string name) {if(!ok) throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static void Main() {
  var action=new ActionCounter();var pp=new DoOnConditionChangedPawnPostProcessor {Condition=new Condition(),Action=action};
  var a=new Pawn {State=false};var b=new Pawn {State=true};
  pp.PostProcess(a,null,null);var ca=Find.Root.Started[0];
  pp.PostProcess(b,null,null);var cb=Find.Root.Started[1];
  ca.Routine.MoveNext();cb.Routine.MoveNext();
  Assert(action.Count(a)==0 && action.Count(b)==0,"different initial states do not trigger each other");
  a.State=true;ca.Routine.MoveNext();cb.Routine.MoveNext();
  Assert(action.Count(a)==1 && action.Count(b)==0,"only changed pawn triggers");
  a.State=false;ca.Routine.MoveNext();ca.Routine.MoveNext();
  Assert(action.Count(a)==2,"both edges trigger once, stable state does not");
  pp.PostProcess(a,null,null);
  Assert(ca.Stopped && !cb.Stopped,"reassignment cancels only same pawn coroutine");
  var replacement=Find.Root.Started[2];pp.PostProcess(a,null,null);
  Assert(replacement.Stopped,"subsequent reassignment cancels replacement");
  int n=Find.Root.Started.Count;new DoOnConditionChangedPawnPostProcessor {Action=action}.PostProcess(a,null,null);
  Assert(n==Find.Root.Started.Count,"missing condition does not launch coroutine");
  var repeat=new DoRepeatPawnPostProcessor {Action=action,DelayHours=1};
  repeat.PostProcess(a,null,null);var ra=Find.Root.Started[Find.Root.Started.Count-1];
  repeat.PostProcess(b,null,null);var rb=Find.Root.Started[Find.Root.Started.Count-1];
  repeat.PostProcess(a,null,null);Assert(ra.Stopped && !rb.Stopped,"repeat cancels only same pawn coroutine");
  Console.WriteLine(checks+" checks passed");Find.Root=null;
 }
}
