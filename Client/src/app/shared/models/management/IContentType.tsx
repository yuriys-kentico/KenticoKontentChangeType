export interface IContentType {
  id: string;
  name: string;
  codename: string;
  elements: {
    id: string;
    type: string;
    name: string;
    codename: string;
    snippet: {
      id: string;
    };
  }[];
}
