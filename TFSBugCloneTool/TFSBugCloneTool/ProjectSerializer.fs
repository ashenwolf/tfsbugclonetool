module ProjectSerializer

open System
open System.IO
open System.Runtime.Serialization
open System.Xml.Serialization

open TfsProjectModels

let private _settingsPath() =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"CSRD\TFSBugCloneTool\settings.xml")

module ProjectSerializer =
  let Serialize(obj) =
    let s = new DataContractSerializer(typeof<TfsProjectModel>)
    let path = _settingsPath()
    Directory.CreateDirectory(Path.GetDirectoryName(path)) |> ignore
    let w = System.Xml.XmlWriter.Create(path)
    s.WriteObject(w, obj)
    w.Close()

  let Deserialize() =
    match File.Exists(_settingsPath()) with
    | true -> 
      let s = new DataContractSerializer(typeof<TfsProjectModel>)
      match s.ReadObject(System.Xml.XmlReader.Create(_settingsPath())) with
      | :? TfsProjectModel as d -> Some(d)
      | _ -> None
    | false -> None
