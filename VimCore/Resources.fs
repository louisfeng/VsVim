﻿#light

namespace Vim

module internal Resources =
    let SelectionTracker_AlreadyRunning = "Already running"
    let SelectionTracker_NotRunning = "Not Running"
    let VisualMode_Banner = "--Visual--"

    let NormalMode_PatternNotFound pattern = sprintf "Pattern not found: %s" pattern
    let NormalMode_NoPreviousSearch = "No previous search"
    let NormalMode_NoWordUnderCursor = "No word under cursor"
    let NormalMode_NoStringUnderCursor = "No string under cursor"

    let CommandMode_InvalidCommand = "Invalid command"