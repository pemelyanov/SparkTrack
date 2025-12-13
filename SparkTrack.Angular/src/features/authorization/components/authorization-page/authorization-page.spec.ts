import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuthorizationPage } from './authorization-page';

describe('AuthorizationPage', () => {
  let component: AuthorizationPage;
  let fixture: ComponentFixture<AuthorizationPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthorizationPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AuthorizationPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
