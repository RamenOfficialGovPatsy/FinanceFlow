DROP TRIGGER IF EXISTS trigger_update_goal_amount ON "GoalDeposits";
DROP FUNCTION IF EXISTS update_goal_current_amount();
DROP TRIGGER IF EXISTS trigger_check_goal_completion ON "Goals";
DROP FUNCTION IF EXISTS check_goal_completion();