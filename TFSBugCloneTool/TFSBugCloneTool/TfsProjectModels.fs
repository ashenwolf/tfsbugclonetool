module TfsProjectModels

type TfsActiveItem = {
  Id: int
  Title: string
  State: string
  Reason: string
  }

type TfsProjectModel(uri, projects: string []) =
  member self.uri = uri
  member self.projects = projects
