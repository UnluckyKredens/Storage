import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoriesPresenter } from './categories-presenter';

describe('CategoriesPresenter', () => {
  let component: CategoriesPresenter;
  let fixture: ComponentFixture<CategoriesPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CategoriesPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoriesPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
