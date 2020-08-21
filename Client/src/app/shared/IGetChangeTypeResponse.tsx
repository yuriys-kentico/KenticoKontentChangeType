import { IContentType } from './models/management/IContentType';

export interface IGetTypesResponse {
  currentType: IContentType;
  otherTypes: IContentType[];
}
