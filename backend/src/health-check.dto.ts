export interface HealthCheckDto {
    /**
     * Human readable time since start
     */
    uptime: string;
    /**
     * NODE_ENV
     */
    environment: string;
    /**
     * Semantic version
     */
    version: string;
    /**
     * Git Hash
     */
    release: string;
    /**
     * Build time, iso time when the build was completed
     */
    buildTime: string;
}
