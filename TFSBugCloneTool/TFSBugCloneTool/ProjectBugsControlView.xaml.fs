namespace ViewModels

open System
open System.Windows
open System.Collections
open System.Collections.ObjectModel
open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

open RelayCommand
open TfsProjectModels
open TfsProxy

type ProjectBugsControlView = XAML<"ProjectBugsControlView.xaml", true>

type IOperationsManager =
    abstract member MoveBugs: seq<TfsActiveItem> -> string -> unit

type BugsListViewModel(uri, projects, operations: IOperationsManager) = 
    inherit ViewModelBase()

    let uri = uri
    let mutable project = if projects |> Seq.length > 0 then projects |> Seq.nth 0 else ""
    let operations = operations

    let mutable reason = ""
    let mutable state = ""
    let mutable items = [||]

    let _GetFilteredItems(reason, state) =
      let filteredItems =
        items
        |> Seq.filter(fun i -> match reason with "" -> true | v -> i.Reason.Equals(v))
        |> Seq.filter(fun i -> match state  with "" -> true | v -> i.State.Equals(v))

      new ObservableCollection<TfsActiveItem>(filteredItems)

    member val Items = ObservableCollection<TfsActiveItem>() with get, set

    member val Reasons = ObservableCollection<string>() with get, set
    member self.SelectedReason
      with get() = reason
      and set(v) =
        reason <- v
        self.Items <- _GetFilteredItems(reason, state)
        self.RaisePropertyChanged("Items")

    member val States = ObservableCollection<string>() with get, set
    member self.SelectedState
      with get() = state
      and set(v) =
        state <- v
        self.Items <- _GetFilteredItems(reason, state)
        self.RaisePropertyChanged("Items")

    member val Projects = new ObservableCollection<string>(projects |> Seq.toList) with get
    member self.SelectedProject
      with get() = project
      and set(v) =
        items <- TfsProxy.RequestBugsForProject(uri, v)
        self.Items <- new ObservableCollection<TfsActiveItem>(items)
        self.RaisePropertyChanged("Items")
        self.Reasons <- new ObservableCollection<string>(items |> Seq.groupBy(fun i -> i.Reason) |> Seq.map(fun (i, _) -> i) |> Seq.distinct |> Seq.append [|""|])
        self.SelectedReason <- ""
        self.RaisePropertyChanged("Reasons")
        self.States  <- new ObservableCollection<string>(items |> Seq.groupBy(fun i -> i.State) |> Seq.map(fun (i, _) -> i) |> Seq.distinct  |> Seq.append [|""|])
        self.SelectedState <- ""
        self.RaisePropertyChanged("States")

    member self.CloneSelectedBugsCommand =
      new RelayCommand(
        (fun _ -> true),
        (fun items -> 
          operations.MoveBugs (seq { for item in items :?> List<TfsActiveItem> do yield item }) project
        ))
