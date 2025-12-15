import { guid } from './guid';

export interface Project {
  id: guid;
  name: string;
  link?: string;
}
