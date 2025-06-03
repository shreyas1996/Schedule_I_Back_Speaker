#!/bin/bash

# Build Configurations Script for BackSpeaker Mod
# This script allows easy building with different logging configurations

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to clean previous builds
clean_build() {
    print_info "Cleaning previous builds..."
    if [ -d "bin" ]; then
        rm -rf bin
        print_info "Removed bin directory"
    fi
    if [ -d "obj" ]; then
        rm -rf obj
        print_info "Removed obj directory"
    fi
}

# Function to build with specific configuration
build_configuration() {
    local config=$1
    local description=$2
    
    print_info "Building with $config configuration..."
    print_info "Description: $description"
    
    dotnet build --configuration $config
    
    if [ $? -eq 0 ]; then
        print_success "$config build completed successfully!"
        
        # Show output location
        if [ -f "bin/$config/net6.0/BackSpeakerMod.dll" ]; then
            print_success "Output: bin/$config/net6.0/BackSpeakerMod.dll"
            
            # Show file size
            local size=$(du -h "bin/$config/net6.0/BackSpeakerMod.dll" | cut -f1)
            print_info "File size: $size"
        fi
    else
        print_error "$config build failed!"
        return 1
    fi
}

# Function to show usage
show_usage() {
    echo "Build Configurations for BackSpeaker Mod"
    echo "Usage: $0 [option]"
    echo ""
    echo "Options:"
    echo "  debug     - Debug build with full logging (LogLevel.Debug)"
    echo "  release   - Release build with minimal logging (LogLevel.Warning)"
    echo "  verbose   - Optimized build with verbose logging (LogLevel.Debug)"
    echo "  minimal   - Production build with error-only logging (LogLevel.Error)"
    echo "  il2cpp    - IL2CPP optimized build with minimal logging"
    echo "  all       - Build all configurations"
    echo "  clean     - Clean all build artifacts"
    echo "  help      - Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 debug     # For development"
    echo "  $0 release   # For production release"
    echo "  $0 verbose   # For troubleshooting in production"
    echo "  $0 clean     # Clean before building"
}

# Main script logic
case "$1" in
    "debug")
        clean_build
        build_configuration "Debug" "Full debug logging, unoptimized code, debug symbols"
        ;;
    "release")
        clean_build
        build_configuration "Release" "Minimal logging (warnings/errors only), optimized code"
        ;;
    "verbose")
        clean_build
        build_configuration "Verbose" "Optimized build with verbose logging (LogLevel.Debug)"
        ;;
    "minimal")
        clean_build
        build_configuration "Minimal" "Production build with error-only logging (LogLevel.Error)"
        ;;
    "il2cpp")
        clean_build
        build_configuration "IL2CPP" "IL2CPP optimized build with minimal logging"
        ;;
    "all")
        print_info "Building all configurations..."
        clean_build
        build_configuration "Debug" "Debug build"
        build_configuration "Release" "Release build"
        build_configuration "IL2CPP" "IL2CPP build"
        print_success "All builds completed!"
        ;;
    "clean")
        clean_build
        print_success "Clean completed!"
        ;;
    "help"|"-h"|"--help")
        show_usage
        ;;
    "")
        print_warning "No configuration specified. Building Release by default..."
        build_configuration "Release" "Default release build"
        ;;
    *)
        print_error "Unknown configuration: $1"
        echo ""
        show_usage
        exit 1
        ;;
esac 