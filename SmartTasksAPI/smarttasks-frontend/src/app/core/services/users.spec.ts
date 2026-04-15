import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { Users } from './users';

describe('Users', () => {
  let service: Users;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient()]
    });
    service = TestBed.inject(Users);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
