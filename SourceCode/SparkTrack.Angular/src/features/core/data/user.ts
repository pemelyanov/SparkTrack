import { guid } from './guid';
import { Role } from '../enums/role';

export interface User {
  id: guid;
  name: string;
  role: Role;
}
