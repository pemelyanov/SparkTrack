import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubTaskCell } from './sub-task-cell';

describe('SubTaskCell', () => {
  let component: SubTaskCell;
  let fixture: ComponentFixture<SubTaskCell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubTaskCell]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubTaskCell);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
