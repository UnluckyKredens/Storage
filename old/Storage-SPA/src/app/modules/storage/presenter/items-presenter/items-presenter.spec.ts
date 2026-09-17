import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItemsPresenter } from './items-presenter';

describe('ItemsPresenter', () => {
  let component: ItemsPresenter;
  let fixture: ComponentFixture<ItemsPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ItemsPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItemsPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
