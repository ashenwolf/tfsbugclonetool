module TfsProxy

open System
open System.Windows.Forms
open Microsoft.TeamFoundation.Client
open Microsoft.TeamFoundation.WorkItemTracking.Client

open TfsProjectModels

let private _queryAll(uri, wiql) =
  try
    let tpc = new TfsTeamProjectCollection(new Uri(uri))
    let workItemStore = tpc.GetService(typeof<WorkItemStore>) :?> WorkItemStore
    let queryResults = workItemStore.Query(wiql)
    seq {for wi in queryResults do yield wi }
      |> Seq.filter(fun wi -> wi.Type.Name.Equals("Bug", StringComparison.InvariantCultureIgnoreCase))
      |> Seq.map(fun wi -> { Id = wi.Id; Title = wi.Title; State = wi.State; Reason = wi.Item("Resolved Reason") :?> string })
      |> Seq.toArray
  with
  | :? Microsoft.TeamFoundation.TeamFoundationServiceUnavailableException as ex -> [||]
  | :? Microsoft.TeamFoundation.WorkItemTracking.Client.UnexpectedErrorException as ex -> [||]
  | :? System.UriFormatException as ex -> [||]

let private _getBugByID(uri, id) =
  

type TfsProxy() =
  static member GetNewTfsServer() =
    using(new TeamProjectPicker(TeamProjectPickerMode.MultiProject, false)) (fun tpp ->
      match tpp.ShowDialog() with
        | DialogResult.OK -> 
          System.Console.WriteLine("Selected Team Project Collection Uri: " + tpp.SelectedTeamProjectCollection.Uri.ToString())
          Some(tpp.SelectedTeamProjectCollection.Uri.ToString(), tpp.SelectedProjects |> Seq.map (fun p -> p.Name) |> Seq.toArray)
        | _ -> None
      )

  static member RequestBugsForProject(uri, project) =
    let q =
      @"Select [Id], [Title], [State], [System.TeamProject], [Resolved Reason] From WorkItems " +
      @"Where " +
      @"[System.TeamProject] = '" + project + "' And [Work Item Type] = 'Bug' "
    _queryAll(uri, q);
