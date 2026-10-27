param(
    [string]$EditorDirectory = 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor',
    [string]$LogPrefix = 'defense-core'
)

$ErrorActionPreference = 'Stop'
$projectDirectory = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$monoExecutable = Join-Path $EditorDirectory 'Data/MonoBleedingEdge/bin/mono.exe'
$compilerExecutable = Join-Path $EditorDirectory 'Data/MonoBleedingEdge/lib/mono/4.5/csc.exe'
if (!(Test-Path -LiteralPath $monoExecutable) -or !(Test-Path -LiteralPath $compilerExecutable))
{
    throw 'Standalone Mono/C# compiler was not found. Pass -EditorDirectory pointing to the installed toolchain.'
}
if ($LogPrefix -notmatch '^[a-zA-Z0-9_-]+$') { throw 'LogPrefix must contain only letters, digits, underscores or hyphens.' }

# Executes pure C# console checks only. Never starts Unity or loads UnityEngine assemblies.
$coreNames = @(
    'DefenseGameState', 'DefenseGameplayClock', 'DefenseRunLifecycle', 'DefenseG1RuntimeAdapter',
    'IDefenseGameplayContext', 'DefenseSession', 'DefenseSpawnJob', 'DefenseSpawnPlanner',
    'DefenseStageSpawnScheduler', 'DefenseDamageContext', 'DefenseTargetIndex', 'DefenseRunStats',
    'DefenseUpgradeDeck', 'DefenseHealthCore', 'DefenseHeroRespawnTracker',
    'DefenseHeroCommands', 'DefenseAbilityRuntime', 'DefenseAimResolver', 'DefenseSkillGestureSession',
    'DefenseSkillDefinition', 'DefenseSkillCastRuntime', 'DefenseActorLifetime', 'DefenseGroundNavigationRules', 'DefenseSpatialRules', 'DefenseLandingFlight', 'DefenseAggroPolicy', 'DefenseStatusRuntime', 'DefenseConeTelegraphRuntime', 'DefenseYakshaBehaviorFSM', 'DefenseIndicatorProjection'
)
$checkNames = @('DefenseSessionChecks', 'DefenseCombatChecks', 'DefenseAbilityInputChecks', 'DefenseGestureAimChecks', 'DefenseSkillCastChecks', 'DefenseVongxuyenControlChecks', 'DefenseActorLifetimeChecks', 'DefenseGroundAggroChecks', 'DefenseSpawnQueueChecks', 'DefenseAirTargetChecks', 'DefenseLandingChecks', 'DefenseMultiAggroChecks', 'DefenseStatusChecks', 'DefenseConeTelegraphChecks', 'DefenseBossFSMChecks', 'DefenseScholarCombatChecks', 'DefenseIndicatorChecks')
$temporaryDirectory = Join-Path $projectDirectory 'Temp'
$logDirectory = Join-Path $projectDirectory 'Logs/TraversalQA'
New-Item -ItemType Directory -Force -Path $temporaryDirectory, $logDirectory | Out-Null
$responsePath = Join-Path $temporaryDirectory 'defense-core-checks.rsp'
$responseLines = @('/nologo', '/target:exe')
$responseLines += '"' + (Join-Path $projectDirectory 'Assets/Features/UI/Controls/SkillPointerGesture.cs') + '"'
foreach ($coreName in $coreNames)
{
    $sourcePath = Join-Path $projectDirectory "Assets/Features/Defense3D/$coreName.cs"
    $responseLines += '"' + $sourcePath + '"'
}
foreach ($checkName in $checkNames)
{
    $responseLines += '"' + (Join-Path $PSScriptRoot "$checkName.cs") + '"'
}
Set-Content -LiteralPath $responsePath -Value $responseLines -Encoding utf8
foreach ($checkName in $checkNames)
{
    $outputPath = Join-Path $temporaryDirectory "$checkName.exe"
    & $monoExecutable $compilerExecutable /noconfig "/main:$checkName" "/out:$outputPath" "@$responsePath"
    if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $checkName" }
    & $monoExecutable $outputPath | Tee-Object -FilePath (Join-Path $logDirectory "$LogPrefix-$checkName.txt")
    if ($LASTEXITCODE -ne 0) { throw "Checks failed: $checkName" }
}
