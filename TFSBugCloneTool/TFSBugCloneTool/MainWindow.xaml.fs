namespace ViewModels

open System
open System.Windows
open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

open TfsProxy
open TfsProjectModels
open RelayCommand
open ProjectSerializer

type MainView = XAML<"MainWindow.xaml", true>

type MainViewModel() as self = 
    inherit ViewModelBase()

    let mutable tfsProjectModel = ProjectSerializer.Deserialize()

    member val LeftProjectBugsModel  = 
      match tfsProjectModel with Some(m) -> BugsListViewModel(m.uri.ToString(), m.projects, self) | None -> BugsListViewModel("", [||], self) 
      with get, set
    member val RightProjectBugsModel = 
      match tfsProjectModel with Some(m) -> BugsListViewModel(m.uri.ToString(), m.projects, self) | None -> BugsListViewModel("", [||], self)
      with get, set

    member self.ConnectToTfsCommand =
      new RelayCommand(
        (fun _ -> true),
        (fun _ ->
          match TfsProxy.GetNewTfsServer() with
          | Some(uri, proj) ->
              tfsProjectModel <- Some(new TfsProjectModel(uri, proj))
              self.LeftProjectBugsModel  <- new BugsListViewModel(uri, proj, self)
              self.RaisePropertyChanged("LeftProjectBugsModel")
              self.RightProjectBugsModel <- new BugsListViewModel(uri, proj, self)
              self.RaisePropertyChanged("RightProjectBugsModel")
              self.Save()
          | None -> ()
          ))
      
    member self.Save() =
      ProjectSerializer.Serialize(tfsProjectModel.Value)

    interface IOperationsManager with
      member self.MoveBugs items project =
        
        false |> ignore
