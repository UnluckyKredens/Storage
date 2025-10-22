import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockPresenter } from './stock-presenter';

describe('StockPresenter', () => {
  let component: StockPresenter;
  let fixture: ComponentFixture<StockPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StockPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
