import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeaturesPage } from './features-page';

describe('FeaturesPage', () => {
  let component: FeaturesPage;
  let fixture: ComponentFixture<FeaturesPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeaturesPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FeaturesPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
