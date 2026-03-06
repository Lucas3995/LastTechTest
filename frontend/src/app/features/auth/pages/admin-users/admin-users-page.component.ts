import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { firstValueFrom } from 'rxjs';

import { AdminFacade } from '../../../../application';
import { AuthService } from '../../../../core';
import { AdminUser, CreateAdminUserPayload } from '../../../../domain';
import { AdminResetDialogComponent } from '../../components/admin-reset-dialog/admin-reset-dialog.component';
import { AdminUserFormDrawerComponent } from '../../components/admin-user-form-drawer/admin-user-form-drawer.component';

@Component({
  selector: 'app-admin-users-page',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatSnackBarModule,
    AdminUserFormDrawerComponent,
  ],
  templateUrl: './admin-users-page.component.html',
  styleUrl: './admin-users-page.component.scss',
})
export class AdminUsersPageComponent implements OnInit {
  private readonly facade = inject(AdminFacade);
  private readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = ['email', 'role', 'actions'];

  isDrawerOpen = false;

  ngOnInit(): void {
    this.loadUsers();
  }

  users(): AdminUser[] {
    return this.facade.users();
  }

  isLoading(): boolean {
    return this.facade.isLoading();
  }

  errorMessage(): string | null {
    return this.facade.errorMessage();
  }

  infoMessage(): string | null {
    return this.facade.infoMessage();
  }

  onSearchInput(term: string): void {
    this.facade.setSearchTerm(term);
  }

  openDrawer(): void {
    this.facade.clearMessages();
    this.isDrawerOpen = true;
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
  }

  async onCreateUser(payload: CreateAdminUserPayload): Promise<void> {
    await this.facade.createUser(payload);

    if (!this.errorMessage()) {
      this.isDrawerOpen = false;
      this.snackBar.open('Usuário criado. Senha inicial: Trocar@123', 'OK', { duration: 5000 });
    }
  }

  async onResetPassword(user: AdminUser): Promise<void> {
    const dialogRef = this.dialog.open(AdminResetDialogComponent, {
      data: { email: user.email },
      width: '420px',
    });

    const confirmed = await firstValueFrom(dialogRef.afterClosed());
    if (!confirmed) {
      return;
    }

    await this.facade.resetPassword({ email: user.email });

    if (!this.errorMessage()) {
      this.snackBar.open('Senha resetada com sucesso para: Trocar@123', 'OK', { duration: 5000 });
    }
  }

  roleLabel(user: AdminUser): string {
    return user.roles.join(', ');
  }

  isSelfUser(user: AdminUser): boolean {
    const current = this.authService.currentUser();
    return !!current && current.id === user.id;
  }

  trackByUserId(_index: number, user: AdminUser): string {
    return user.id;
  }

  private loadUsers(): void {
    this.facade.loadUsers();
  }
}
