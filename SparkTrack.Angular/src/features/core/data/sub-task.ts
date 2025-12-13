import { guid } from './guid';
import { User } from './user';

export interface SubTask {
  id: guid;
  name: string;
  employee: User;
  cost: number;
  isCompleted: boolean;
  onPayment: boolean;
}
