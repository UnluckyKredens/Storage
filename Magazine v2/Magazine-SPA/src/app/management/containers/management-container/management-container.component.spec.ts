import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../environments/environment';

import { ManagementContainerComponent } from './management-container.component';

describe('ManagementContainerComponent', () => {
  let component: ManagementContainerComponent;
  let fixture: ComponentFixture<ManagementContainerComponent>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManagementContainerComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ManagementContainerComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http
      .expectOne(`${environment.apiUrl}/Auth/me`)
      .flush({ roleId: '10000000-0000-0000-0000-000000000001' });
    http.expectOne(`${environment.apiUrl}/Auth/me/permissions`).flush({ permissionCodes: [] });
  });

  afterEach(() => http.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
