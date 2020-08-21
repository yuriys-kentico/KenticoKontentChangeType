import { IContentItem } from './models/management/IContentItem';

export interface IChangeTypeResponse {
  totalApiCalls: number;
  totalMilliseconds: number;
  newItem: IContentItem;
  updatedItems: IContentItem[];
}
