import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AnticipationSimulationFormComponent } from '../../components/anticipation-simulation-form/anticipation-simulation-form.component';
import { AnticipationSimulationResultPanelComponent } from '../../components/anticipation-simulation-result-panel/anticipation-simulation-result-panel.component';
import { ConfirmConversionDialogComponent } from '../../components/confirm-conversion-dialog/confirm-conversion-dialog.component';
import { AnticipationSimulationFacade } from '../../../../application/anticipation/anticipation-simulation.facade';
import { AuthService } from '../../../../core/auth/auth.service';
import { SimulateAnticipationPayload } from '../../../../domain';

@Component({
  selector: 'app-anticipation-simulation-page',
  standalone: true,
  imports: [
    CommonModule,
    AnticipationSimulationFormComponent,
    AnticipationSimulationResultPanelComponent,
  ],
  templateUrl: './anticipation-simulation-page.component.html',
  styleUrl: './anticipation-simulation-page.component.scss',
  providers: [AnticipationSimulationFacade]
})
export class AnticipationSimulationPageComponent implements OnInit {

  private facade = inject(AnticipationSimulationFacade);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  // Signals proxied from facade (for template use)
  simulationResult = this.facade.simulationResult;
  loading = this.facade.loading;
  errorMessage = this.facade.errorMessage;
  infoMessage = this.facade.infoMessage;

  // Writable properties exposed for tests (tests set them directly)
  userRole = '';
  selectedCreatorId: string | null = null;
  isSubmitting = false;
  canConvert = false;
  errorSupportId: string | null = null;

  ngOnInit(): void {
    // Resolve user role from auth service
    const role = this.authService.currentUser()?.role;
    if (role) {
      this.userRole = role;
    }

    // Reset facade on entering the page
    this.facade.reset();

    // If Admin/Analyst has a creatorId in query params, pre-populate
    const creatorId = this.route.snapshot.queryParams['creatorId'];
    if (creatorId) {
      this.facade.setCreatorIdForSimulation(creatorId);
    }
  }

  async onSimulate(payload: SimulateAnticipationPayload): Promise<void> {
    this.isSubmitting = true;
    await this.facade.simulate(payload);
    this.isSubmitting = false;
    this.canConvert = this.facade.canConvert();
    this.errorSupportId = this.facade.errorSupportId();
  }

  async onConvertClick(): Promise<void> {
    // Open MatDialog for confirmation instead of browser confirm()
    const dialogRef = this.dialog.open(ConfirmConversionDialogComponent, {
      data: this.facade.simulationResult(),
      width: '400px',
    });

    const confirmed = await dialogRef.afterClosed().toPromise();
    if (confirmed) {
      await this.facade.convertToRealRequest();
      this.canConvert = this.facade.canConvert();
    }
  }

  onCreatorSelected(creatorId: string): void {
    this.selectedCreatorId = creatorId;
    this.facade.setCreatorIdForSimulation(creatorId);
  }

  /** Returns creatorIdForSimulation as string (empty string if null). */
  get creatorIdForSimulation(): string {
    return this.facade.creatorIdForSimulation() ?? '';
  }

  /** Returns isConverting signal. */
  get isConverting(): boolean {
    return this.facade.isConverting();
  }
}